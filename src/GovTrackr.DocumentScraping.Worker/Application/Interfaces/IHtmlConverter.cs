namespace GovTrackr.DocumentScraping.Worker.Application.Interfaces;

internal interface IHtmlConverter
{
    public string Convert(string html);
}