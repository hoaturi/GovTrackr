using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using ReverseMarkdown;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Converters;

public class HtmlToMarkdownConverter : IHtmlConverter
{
    private readonly Converter _converter;

    public HtmlToMarkdownConverter()
    {
        var config = new Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            UnknownTags = Config.UnknownTagsOption.Drop
        };

        _converter = new Converter(config);
    }

    public string Convert(string htmlContent)
    {
        return _converter.Convert(htmlContent);
    }
}