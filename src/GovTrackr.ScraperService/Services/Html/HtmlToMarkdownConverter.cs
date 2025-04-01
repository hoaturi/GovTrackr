using GovTrackr.ScraperService.Contracts.Html;
using ReverseMarkdown;

namespace GovTrackr.ScraperService.Services.Html;

public class HtmlToMarkdownConverter : IHtmlConverter
{
    private readonly Converter _converter;

    // Constructor: Initialize the ReverseMarkdown converter here
    public HtmlToMarkdownConverter()
    {
        var config = new Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            UnknownTags = Config.UnknownTagsOption.PassThrough
        };

        _converter = new Converter(config);
    }

    public string Convert(string htmlContent)
    {
        return _converter.Convert(htmlContent);
    }
}