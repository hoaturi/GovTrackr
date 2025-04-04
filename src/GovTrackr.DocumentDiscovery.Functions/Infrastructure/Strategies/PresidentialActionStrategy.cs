using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Shared.Abstractions.Browser;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using Shared.MessageContracts;

namespace GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies;

public class PresidentialActionStrategy(
    AppDbContext dbContext,
    IBrowserService browserService,
    ILogger<PresidentialActionStrategy> logger)
    : IDiscoveryStrategy
{
    private const string BaseSourceUrl = "https://www.whitehouse.gov/presidential-actions/";
    private const string LinkSelector = "ul.wp-block-post-template li h2.wp-block-post-title a";
    private const int MaxRetryAttempts = 2;
    private const int InitialRetryDelay = 1000;

    public async Task<DocumentDiscovered?> DiscoverAsync(CancellationToken cancellationToken)
    {
        var allNewLinksDiscovered = new List<string>();
        var pageNumber = 1;
        IPage? page = null;

        try
        {
            page = await browserService.GetPageAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                var currentPageUrl = pageNumber == 1 ? BaseSourceUrl : $"{BaseSourceUrl}page/{pageNumber}/";
                logger.LogInformation("Processing page {PageNumber}: {Url}", pageNumber, currentPageUrl);

                var response = await NavigateToPageWithRetryAsync(page, currentPageUrl, pageNumber, cancellationToken);
                if (response is not { Ok: true }) return null;

                var linksOnCurrentPage = await page.Locator(LinkSelector).AllAsync();
                if (!linksOnCurrentPage.Any())
                {
                    logger.LogInformation("No links found on page {PageNumber}, stopping pagination.", pageNumber);
                    break;
                }

                var (foundDuplicate, newLinks) =
                    await ProcessLinksOnPageAsync(linksOnCurrentPage, pageNumber, cancellationToken);
                allNewLinksDiscovered.AddRange(newLinks);

                if (foundDuplicate)
                {
                    logger.LogInformation("Duplicate link encountered on page {PageNumber}, stopping pagination.",
                        pageNumber);
                    break;
                }

                pageNumber++;
            }

            return new DocumentDiscovered
            {
                DocumentCategory = DocumentCategoryType.PresidentialAction,
                Urls = allNewLinksDiscovered
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the discovery process: {Message}", ex.Message);
            return null;
        }
        finally
        {
            if (page is not null) await browserService.ClosePageAsync(page);
        }
    }

    private async Task<IResponse?> NavigateToPageWithRetryAsync(
        IPage page,
        string url,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var retryCount = 0;
        var delay = InitialRetryDelay;

        while (retryCount < MaxRetryAttempts)
        {
            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            if (response is { Status: >= 500 })
            {
                logger.LogWarning(
                    "Retry {RetryCount}/{MaxRetries}: Server error {Status} on page {PageNumber}. Retrying in {Delay}ms...",
                    retryCount + 1, MaxRetryAttempts, response.Status, pageNumber, delay);

                await Task.Delay(delay, cancellationToken);
                delay *= 2; // Exponential backoff
                retryCount++;
            }
            else
            {
                return response; // Success
            }
        }

        logger.LogWarning("Page {PageNumber} failed after retries, stopping pagination.", pageNumber);
        return null;
    }

    private async Task<(bool FoundDuplicate, List<string> NewLinks)> ProcessLinksOnPageAsync(
        IReadOnlyList<ILocator> linksOnCurrentPage,
        int pageNumber,
        CancellationToken cancellationToken)
    {
        var hrefs = await ExtractHrefsAsync(linksOnCurrentPage);

        if (hrefs.Count == 0)
        {
            logger.LogWarning("No href attributes found on page {PageNumber}.", pageNumber);
            return (false, []);
        }

        // Convert to absolute URLs and filter out null/empty values
        var absoluteUrls = ConvertToAbsoluteUrls(hrefs);

        if (absoluteUrls.Count != 0) return await CheckForDuplicatesAsync(absoluteUrls, cancellationToken);

        logger.LogWarning("No valid href attributes found on page {PageNumber}.", pageNumber);
        return (false, []);
    }

    private static async Task<List<string>> ExtractHrefsAsync(IReadOnlyList<ILocator> linksOnCurrentPage)
    {
        // Fetch href attributes in parallel
        var hrefTasks = linksOnCurrentPage.Select(link => link.GetAttributeAsync("href")).ToList();
        var hrefs = await Task.WhenAll(hrefTasks);
        return hrefs.OfType<string>().ToList();
    }

    private static List<string> ConvertToAbsoluteUrls(List<string> hrefs)
    {
        return hrefs
            .Select(href => Uri.TryCreate(new Uri(BaseSourceUrl), href, out var uri) ? uri.ToString() : null)
            .OfType<string>()
            .ToList();
    }

    private async Task<(bool FoundDuplicate, List<string> NewLinks)> CheckForDuplicatesAsync(
        List<string> absoluteUrls,
        CancellationToken cancellationToken)
    {
        var existingUrls = await dbContext.PresidentialActions.AsNoTracking()
            .Where(pa => absoluteUrls.Contains(pa.SourceUrl))
            .Select(pa => pa.SourceUrl)
            .ToListAsync(cancellationToken);

        var newLinks = new List<string>();
        var foundDuplicate = false;

        foreach (var url in absoluteUrls)
        {
            if (existingUrls.Contains(url))
            {
                foundDuplicate = true;
                break;
            }

            newLinks.Add(url);
        }

        return (foundDuplicate, newLinks);
    }
}