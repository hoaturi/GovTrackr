using GovTrackr.Digest.Functions.Application.Dtos;
using GovTrackr.Digest.Functions.Application.Interfaces;
using Mjml.Net;
using Scriban;

namespace GovTrackr.Digest.Functions.Infrastructure.Email;

public class DigestEmailBuilder : IDigestEmailBuilder
{
    private const string TemplateFileName = "DigestTemplate.mjml";
    private const string TemplateFolderPath = "Infrastructure/Email/Templates";
    private readonly string _digestTemplate;
    private readonly IMjmlRenderer _mjmlRenderer;

    public DigestEmailBuilder(IMjmlRenderer mjmlRenderer)
    {
        _mjmlRenderer = mjmlRenderer;

        var path = Path.Combine(AppContext.BaseDirectory, TemplateFolderPath, TemplateFileName);
        if (!File.Exists(path))
            throw new FileNotFoundException($"The MJML template file was not found at the specified path: {path}");

        var template = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(template))
            throw new InvalidDataException($"The MJML template at {path} is empty or invalid.");

        _digestTemplate = template;
    }

    public async Task<string> GetDigestContentAsync(DigestEmailTemplateDto dto, CancellationToken cancellationToken)
    {
        var template = Template.Parse(_digestTemplate);
        var model = CreateTemplateModel(dto);
        var renderedTemplate = await template.RenderAsync(model);

        var renderResult =
            await _mjmlRenderer.RenderAsync(renderedTemplate, new MjmlOptions { Beautify = false }, cancellationToken);
        return renderResult.Html;
    }

    public string InjectUnsubscribeLink(string template, string token)
    {
        return template.Replace("{{unsubscribe_token}}", token);
    }

    private static object CreateTemplateModel(DigestEmailTemplateDto dto)
    {
        return new
        {
            start_date = dto.StartDate.ToString("yyyy-MM-dd"),
            today_date = dto.TodayDate.ToString("yyyy-MM-dd"),
            presidential_actions = dto.PresidentialActions.Select(action => new
            {
                id = action.Id,
                title = action.Title,
                summary = action.Summary,
                source_url = action.SourceUrl,
                published_at = action.PublishedAt.ToString("yyyy-MM-dd")
            }).ToList(),
            current_year = dto.TodayDate.Year
        };
    }
}