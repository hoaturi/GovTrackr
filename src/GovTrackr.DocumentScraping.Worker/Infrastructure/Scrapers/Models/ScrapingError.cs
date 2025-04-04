using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;

internal record ScrapingError(
    DocumentInfo Document,
    string Message
);