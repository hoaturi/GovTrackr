using FluentResults;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;

internal class ScrapingError : Error
{
    internal ScrapingError(string url, string message) : base(message)
    {
        Metadata.Add("Url", url);
    }
}