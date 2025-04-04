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
    ILogger<PresidentialActionStrategy> logger)
    : IDocumentDiscoveryStrategy
{
    private const string BaseSourceUrl = "https://www.whitehouse.gov/presidential-actions/";
    private const string ItemSelector = "ul.wp-block-post-template li";
    private const string TitleSelector = "h2.wp-block-post-title";
    private const string LinkSelectorWithinItem = $"{TitleSelector} a";

    public async Task<DocumentDiscovered?> DiscoverDocumentsAsync(CancellationToken cancellationToken)
    {
        var discoveredDocuments = new List<DocumentInfo>();
        var pageNumber = 1;
        IPage? page = null;

        try
        {
            page = await browserService.GetPageAsync();

            // Paginate through all the list of documents until we reach the end
            while (!cancellationToken.IsCancellationRequested)
            {
                var url = pageNumber == 1 ? BaseSourceUrl : $"{BaseSourceUrl}page/{pageNumber}/";
                logger.LogInformation("Processing page {PageNumber}: {Url}", pageNumber, url);

                var response = await page.GotoAsync(url);

                if (response is { Ok: false })
                {
                    // If the page is not found (404) and it's not the first page, assume pagination is done
                    if (response.Status == 404 && pageNumber > 1)
                    {
                        logger.LogInformation("Received 404 on page {PageNumber}, assuming end of pagination.",
                            pageNumber);
                        break;
                    }

                    logger.LogWarning("Navigation returned non-OK status {Status} on page {PageNumber}.",
                        response.Status, pageNumber);

                    // Critical if failed on the first page — discovery can't continue
                    if (pageNumber == 1)
                    {
                        logger.LogError("Failed to load the first page. Aborting discovery.");
                        return null;
                    }

                    break;
                }

                // Get all the list items on the page
                var items = await page.Locator(ItemSelector).AllAsync();
                if (!items.Any())
                {
                    logger.LogInformation("No items found on page {PageNumber}. Ending pagination.", pageNumber);
                    break;
                }

                // Extract and process the documents
                var (foundDuplicate, newDocs) = await ProcessPageItemsAsync(items, pageNumber, cancellationToken);
                if (newDocs.Count == 0)
                {
                    logger.LogInformation("No new documents found on page {PageNumber}. Ending pagination.",
                        pageNumber);
                    break;
                }

                discoveredDocuments.AddRange(newDocs);

                // Stop pagination if duplicates are found
                if (foundDuplicate)
                {
                    logger.LogInformation("Duplicate found on page {PageNumber}. Ending pagination.", pageNumber);
                    break;
                }

                pageNumber++;
            }

            logger.LogInformation("Finished discovery. Discovered {Count} new documents.", discoveredDocuments.Count);
            return new DocumentDiscovered
            {
                DocumentCategory = DocumentCategoryType.PresidentialAction,
                Documents = discoveredDocuments
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the discovery process.");
            return null;
        }
        finally
        {
            if (page is not null) await browserService.ClosePageAsync(page);
        }
    }

    // Processes all items on the page, extracting and filtering for duplicates
    private async Task<(bool FoundDuplicate, List<DocumentInfo> NewDocuments)> ProcessPageItemsAsync(
        IReadOnlyList<ILocator> items,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var docs = await ExtractDocumentsAsync(items, pageNumber);

        if (docs.Count == 0) return (false, []);

        var (foundDuplicate, newDocuments) = await FilterDuplicatesAsync(docs, cancellationToken);
        return foundDuplicate ? (true, newDocuments) : (false, newDocuments);
    }

    // Extracts document info (URL + title) from list items on current page
    private async Task<List<DocumentInfo>> ExtractDocumentsAsync(
        IReadOnlyList<ILocator> items,
        int pageNumber)
    {
        var documents = new List<DocumentInfo>();
        var baseUri = new Uri(BaseSourceUrl);

        foreach (var item in items)
        {
            var link = await item.Locator(LinkSelectorWithinItem).GetAttributeAsync("href");
            var title = (await item.Locator(TitleSelector).InnerTextAsync()).Trim();

            if (string.IsNullOrWhiteSpace(link))
            {
                logger.LogWarning("Missing href in item on page {PageNumber}.", pageNumber);
                continue;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                logger.LogWarning("Missing title in item (URL: {Url}) on page {PageNumber}.", link, pageNumber);
                continue;
            }

            if (Uri.TryCreate(baseUri, link, out var absoluteUri))
                documents.Add(new DocumentInfo(absoluteUri.ToString(), title));
            else
                logger.LogWarning("Invalid URI with base {Base} and relative {Relative} on item, page {PageNumber}.",
                    BaseSourceUrl, link, pageNumber);
        }

        return documents;
    }

    // Filters out documents that already exist in the database, and stop pagination if duplicates are found
    private async Task<(bool FoundDuplicate, List<DocumentInfo> NewDocuments)> FilterDuplicatesAsync(
        List<DocumentInfo> docs,
        CancellationToken cancellationToken)
    {
        var urls = docs.Select(d => d.Url).ToList();

        if (urls.Count == 0) return (false, []);

        var existingUrls = await dbContext.PresidentialActions.AsNoTracking()
            .Where(pa => urls.Contains(pa.SourceUrl))
            .Select(pa => pa.SourceUrl)
            .ToListAsync(cancellationToken);

        var newDocs = new List<DocumentInfo>();
        var existingSet = new HashSet<string>(existingUrls);

        foreach (var doc in docs)
        {
            if (existingSet.Contains(doc.Url))
                return (true, newDocs); // Stop immediately if a duplicate is found

            newDocs.Add(doc);
        }

        return (false, newDocs);
    }
}