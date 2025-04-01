namespace GovTrackr.ScraperService.Contracts.Html;

internal interface IHtmlConverter
{
    public string Convert(string html);
}