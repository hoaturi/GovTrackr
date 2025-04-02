namespace GovTrackr.ScrapingService.Abstractions;

internal interface IHtmlConverter
{
    public string Convert(string html);
}