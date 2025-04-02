namespace GovTrackr.ScraperService.Abstractions;

internal interface IHtmlConverter
{
    public string Convert(string html);
}