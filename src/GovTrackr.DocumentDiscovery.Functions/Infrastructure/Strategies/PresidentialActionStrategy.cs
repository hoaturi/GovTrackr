using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Shared.Application.Interfaces;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies;

internal class PresidentialActionStrategy(
    AppDbContext dbContext,
    IBrowserService browserService
) : IDocumentDiscoveryStrategy
{
    private const string BaseUrl = "https://www.whitehouse.gov/presidential-actions/";
    private const string ItemSelector = "ul.wp-block-post-template li";
    private const string TitleSelector = "h2.wp-block-post-title";
    private const string LinkSelector = $"{TitleSelector} a";

    // Paginate through presidential action listings until a duplicate is found or cancellation is requested.
    public async Task<DiscoveryResult> DiscoverDocumentsAsync(CancellationToken cancellationToken)
    {
        var discovered = new List<DocumentInfo>();
        var errors = new List<DiscoveryError>();
        IPage? page = null;

        try
        {
            page = await browserService.GetPageAsync();
            var pageNumber = 1;

            while (!cancellationToken.IsCancellationRequested)
            {
                var url = pageNumber == 1 ? BaseUrl : $"{BaseUrl}page/{pageNumber}/";

                var response = await page.GotoAsync(url);

                // If the page failed to load, capture an error and stop processing.
                if (response is null || !response.Ok)
                {
                    var message = response is null
                        ? "Response was null."
                        : $"Unexpected response. Status: {response.Status}, URL: {response.Url}";

                    errors.Add(new DiscoveryError(url, message));
                    break;
                }

                var items = await page.Locator(ItemSelector).AllAsync();

                // No posts found on this page — stop pagination.
                if (items.Count == 0) break;

                var (foundDuplicate, newDocs, pageErrors) =
                    await ExtractDocumentsFromPageAsync(items, cancellationToken);
                errors.AddRange(pageErrors);
                discovered.AddRange(newDocs);

                // Stop if a previously seen document is found (we've caught up).
                if (foundDuplicate) break;

                pageNumber++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new DiscoveryError(BaseUrl, $"Unhandled exception: {ex.Message}"));
        }
        finally
        {
            if (page is not null)
                await browserService.ClosePageAsync(page);
        }

        return new DiscoveryResult(
            DocumentCategoryType.PresidentialAction,
            discovered,
            errors
        );
    }

    private async Task<(bool FoundDuplicate, List<DocumentInfo> NewDocuments, List<DiscoveryError> Errors)>
        ExtractDocumentsFromPageAsync(IReadOnlyList<ILocator> items, CancellationToken cancellationToken)
    {
        var baseUri = new Uri(BaseUrl);
        var documents = new List<DocumentInfo>();
        var errors = new List<DiscoveryError>();

        foreach (var item in items)
        {
            var link = await item.Locator(LinkSelector).GetAttributeAsync("href");
            var title = (await item.Locator(TitleSelector).InnerTextAsync()).Trim();

            if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(title))
            {
                errors.Add(new DiscoveryError(link ?? "unknown", "Document link or title is missing."));
                continue;
            }

            if (Uri.TryCreate(baseUri, link, out var fullUrl))
                documents.Add(new DocumentInfo(fullUrl.ToString(), title));
            else
                errors.Add(new DiscoveryError(link, "Invalid URL format."));
        }

        if (documents.Count == 0)
            return (false, [], errors);

        var (foundDuplicate, newDocs) = await FilterDuplicatesAsync(documents, cancellationToken);
        return (foundDuplicate, newDocs, errors);
    }

    // Filter out any documents already seen in the database.
    // Stop immediately if a duplicate is found (we assume all newer items have been processed).
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
            if (existingSet.Contains(doc.Url))
                return (true, newDocs);

            newDocs.Add(doc);
        }

        return (false, newDocs);
    }
}