namespace GovTrackr.ScraperService.Abstractions.HtmlProcessing;

internal interface IHtmlToMarkdownConverter
{
    public string Convert(string html);
}