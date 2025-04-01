using GovTrackr.ScraperService.Abstractions.HtmlProcessing;
using ReverseMarkdown;

namespace GovTrackr.ScraperService.Services.HtmlProcessing;

public class HtmlToMarkdownConverter : IHtmlToMarkdownConverter
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