using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Shared.Abstractions.Browser;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies;

internal class PresidentialActionStrategy(
    AppDbContext dbContext,
    IBrowserService browserService,
    ILogger<PresidentialActionStrategy> logger
) : IDocumentDiscoveryStrategy
{
    private const string BaseUrl = "https://www.whitehouse.gov/presidential-actions/";
    private const string ItemSelector = "ul.wp-block-post-template li";
    private const string TitleSelector = "h2.wp-block-post-title";
    private const string LinkSelector = $"{TitleSelector} a";

    public async Task<DocumentDiscovered?> DiscoverDocumentsAsync(CancellationToken cancellationToken)
    {
        var discovered = new List<DocumentInfo>();
        IPage? page = null;

        try
        {
            page = await browserService.GetPageAsync();
            var pageNumber = 1;

            while (!cancellationToken.IsCancellationRequested)
            {
                var url = pageNumber == 1 ? BaseUrl : $"{BaseUrl}page/{pageNumber}/";

                var response = await page.GotoAsync(url);
                if (response is null || !response.Ok)
                {
                    HandleInvalidResponse(response, pageNumber);
                    break;
                }

                var items = await page.Locator(ItemSelector).AllAsync();
                if (items.Count == 0) break;

                var (foundDuplicate, newDocs) = await ProcessPageAsync(items, pageNumber, cancellationToken);
                if (newDocs.Count == 0 || foundDuplicate) break;

                discovered.AddRange(newDocs);
                pageNumber++;
            }

            return discovered.Count == 0
                ? null
                : new DocumentDiscovered
                {
                    DocumentCategory = DocumentCategoryType.PresidentialAction,
                    Documents = discovered
                };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during document discovery.");
            return null;
        }
        finally
        {
            if (page is not null)
                await browserService.ClosePageAsync(page);
        }
    }

    private void HandleInvalidResponse(IResponse? response, int pageNumber)
    {
        if (response is null)
        {
            logger.LogError("Page {PageNumber}: response was null.", pageNumber);
            return;
        }

        if (pageNumber == 1)
        {
            logger.LogError("Failed to load first page. Status: {Status}.", response.Status);
            return;
        }

        if (response.Status == 404) return;

        logger.LogWarning(
            "Unexpected status code on page {PageNumber}. Status: {Status}, URL: {Url}.",
            pageNumber, response.Status, response.Url);
    }

    private async Task<(bool FoundDuplicate, List<DocumentInfo> NewDocuments)> ProcessPageAsync(
        IReadOnlyList<ILocator> items,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var documents = await ExtractDocumentsAsync(items, pageNumber);
        return documents.Count == 0
            ? (false, [])
            : await FilterDuplicatesAsync(documents, cancellationToken);
    }

    private async Task<List<DocumentInfo>> ExtractDocumentsAsync(
        IReadOnlyList<ILocator> items,
        int pageNumber)
    {
        var documents = new List<DocumentInfo>();
        var baseUri = new Uri(BaseUrl);

        foreach (var item in items)
        {
            var link = await item.Locator(LinkSelector).GetAttributeAsync("href");
            var title = (await item.Locator(TitleSelector).InnerTextAsync()).Trim();

            if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(title))
            {
                logger.LogWarning("Invalid item: missing title or link on page {PageNumber}.", pageNumber);
                continue;
            }

            if (Uri.TryCreate(baseUri, link, out var fullUrl))
                documents.Add(new DocumentInfo(fullUrl.ToString(), title));
            else
                logger.LogWarning(
                    "Failed to resolve document URL: base '{Base}' + relative '{Relative}' on page {PageNumber}.",
                    BaseUrl, link, pageNumber);
        }

        return documents;
    }

    private async Task<(bool FoundDuplicate, List<DocumentInfo> NewDocuments)> FilterDuplicatesAsync(
        List<DocumentInfo> docs,
        CancellationToken cancellationToken)
    {
        var urls = docs.Select(d => d.Url).ToList();

        var existingUrls = await dbContext.PresidentialActions.AsNoTracking()
            .Where(pa => urls.Contains(pa.SourceUrl))
            .Select(pa => pa.SourceUrl)
            .ToListAsync(cancellationToken);

        var existingSet = new HashSet<string>(existingUrls);
        var newDocs = new List<DocumentInfo>();

        foreach (var doc in docs)
        {
            // Exit early upon finding the first existing document
            if (existingSet.Contains(doc.Url))
                return (true, newDocs);

            newDocs.Add(doc);
        }

        return (false, newDocs);
    }
}