using System.Collections.Concurrent;
using System.Text;
using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using GovTrackr.DocumentScraping.Worker.Configurations.Options;
using GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers.Models;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Shared.Abstractions.Browser;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers;

internal class PresidentialActionScraper(
    AppDbContext dbContext,
    ILogger<PresidentialActionScraper> logger,
    IHtmlConverter markdownConverter,
    IBrowserService browserService,
    IOptions<ScrapersOptions> options
)
    : IScraper
{
    private const string TitleSelector = ".wp-block-whitehouse-topper__headline";
    private const string CategorySelector = ".wp-block-whitehouse-byline-subcategory__link";
    private const string DateSelector = ".wp-block-post-date time";
    private const string ContentContainerSelector = ".entry-content.wp-block-post-content";

    public async Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken)
    {
        if (urls.Count == 0) return;

        var result = new ScrapingResult();

        // Use SemaphoreSlim to control concurrency
        using var semaphore = new SemaphoreSlim(options.Value.MaxConcurrentPages);
        var tasks = new List<Task>();

        // Create a thread-safe collections for successful documents and failures
        var successfulDocs = new ConcurrentBag<PresidentialAction>();
        var failureList = new ConcurrentBag<ScrapingError>();

        foreach (var url in urls)
        {
            await semaphore.WaitAsync(cancellationToken);

            // Create a new task for each URL with new thread
            var task = Task.Run(async () =>
            {
                var page = await browserService.GetPageAsync();
                try
                {
                    var (document, errorMessage) = await ScrapeDocumentAsync(page, url);
                    if (document is not null)
                        successfulDocs.Add(document);
                    else if (errorMessage is not null)
                        failureList.Add(new ScrapingError(url, errorMessage));
                }
                catch (Exception ex)
                {
                    failureList.Add(new ScrapingError(url, ex.Message));
                }
                finally
                {
                    await browserService.ClosePageAsync(page);
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Transfer results to the result object
        result.Successful.AddRange(successfulDocs);
        foreach (var failure in failureList) result.Failures.Add(failure);

        // Save results to database
        if (result.Successful.Count > 0)
        {
            await dbContext.PresidentialActions.AddRangeAsync(result.Successful, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully scraped and saved {Count} of {TotalCount} documents",
                result.Successful.Count, urls.Count);
        }

        if (result.Failures.Count > 0)
            logger.LogWarning("Failed to scrape {FailureCount} of {TotalCount} documents: {@Failures}",
                result.Failures.Count,
                urls.Count,
                result.Failures.Select(f => new { f.Message }));
    }

    private async Task<(PresidentialAction? Document, string? ErrorMessage)> ScrapeDocumentAsync(IPage page, string url)
    {
        var response = await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (response is not { Ok: true }) return (null, "Failed to load page");

        var title = await ExtractTextAsync(page, TitleSelector);
        if (string.IsNullOrEmpty(title)) return (null, "Failed to extract title");

        var category = await ExtractTextAsync(page, CategorySelector);
        var contentHtml = await ExtractContentAsync(page);
        if (string.IsNullOrEmpty(contentHtml)) return (null, "Failed to extract content");

        var categoryType = ParseCategoryType(category, title, contentHtml);
        if (categoryType is null) return (null, $"Failed to extract or infer category for {url}");

        var publicationDate = await ExtractDateTimeInUtcAsync(page);
        if (publicationDate is null) return (null, "Failed to extract publication date");

        return (new PresidentialAction
        {
            SubCategoryId = (int)categoryType.Value,
            Title = title,
            Content = markdownConverter.Convert(contentHtml),
            SourceUrl = url,
            PublishedAt = publicationDate.Value,
            TranslationStatus = TranslationStatus.Pending
        }, null);
    }

    private static async Task<string?> ExtractTextAsync(IPage page, string selector)
    {
        var element = page.Locator(selector);
        if (await element.CountAsync() == 0)
            return null;

        var text = await element.InnerTextAsync();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static DocumentSubCategoryType? ParseCategoryType(string? category, string title, string content)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            switch (normalizedCategory)
            {
                case "Executive Orders":
                    return DocumentSubCategoryType.ExecutiveOrder;
                case "Presidential Memoranda":
                    return DocumentSubCategoryType.Memoranda;
                case "Proclamations":
                    return DocumentSubCategoryType.Proclamation;
                case "Nominations & Appointments":
                    return DocumentSubCategoryType.Nomination;
            }
        }

        // Fallback: try to infer from title or content
        if (title.Contains("Memorandum", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("Memorandum", StringComparison.OrdinalIgnoreCase))
            return DocumentSubCategoryType.Memoranda;
        if (content.Contains("By the authority vested in me", StringComparison.OrdinalIgnoreCase))
            return DocumentSubCategoryType.ExecutiveOrder;
        if (content.Contains("Proclamation", StringComparison.OrdinalIgnoreCase))
            return DocumentSubCategoryType.Proclamation;

        if (title.Contains("appointment", StringComparison.OrdinalIgnoreCase) ||
            title.Contains("nomination", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("appointment", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("appointments", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("nomination", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("nominations", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("appointed", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("appointee", StringComparison.OrdinalIgnoreCase))
            return DocumentSubCategoryType.Nomination;

        return null;
    }

    private static async Task<string?> ExtractContentAsync(IPage page)
    {
        var contentContainer = page.Locator(ContentContainerSelector);
        if (await contentContainer.CountAsync() == 0) return null;

        var contentParagraphs = contentContainer.Locator("p");
        var paragraphCount = await contentParagraphs.CountAsync();
        if (paragraphCount == 0) return null;

        var contentBuilder = new StringBuilder();
        for (var i = 0; i < paragraphCount; i++)
        {
            var paragraph = contentParagraphs.Nth(i);
            var paragraphHtml = await paragraph.InnerHTMLAsync();
            contentBuilder.Append(paragraphHtml);
        }

        return contentBuilder.ToString().Trim();
    }

    private static async Task<DateTime?> ExtractDateTimeInUtcAsync(IPage page)
    {
        var element = page.Locator(DateSelector);
        if (await element.CountAsync() == 0) return null;

        var dateText = await element.InnerTextAsync();
        if (string.IsNullOrWhiteSpace(dateText)) return null;

        if (DateTimeOffset.TryParse(dateText, out var parsedDate))
            return parsedDate.UtcDateTime;

        var dateAttribute = await element.GetAttributeAsync("datetime");
        if (!string.IsNullOrWhiteSpace(dateAttribute) && DateTimeOffset.TryParse(dateAttribute, out parsedDate))
            return parsedDate.UtcDateTime;

        return null;
    }
}