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
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers;

internal class PresidentialActionScraper(
    ILogger<PresidentialActionScraper> logger,
    IHtmlConverter markdownConverter,
    IBrowserService browserService,
    IOptions<ScrapersOptions> options
) : IScraper
{
    private const string CategorySelector = ".wp-block-whitehouse-byline-subcategory__link";
    private const string DateSelector = ".wp-block-post-date time";
    private const string ContentContainerSelector = ".entry-content.wp-block-post-content";

    public async Task<ScrapingResult> ScrapeAsync(List<DocumentInfo> documents, CancellationToken cancellationToken)
    {
        var result = new ScrapingResult();

        if (documents.Count == 0)
        {
            logger.LogWarning("No documents to scrape.");
            return result;
        }

        // Use Semaphore to limit concurrent browser page usage
        using var semaphore = new SemaphoreSlim(options.Value.MaxConcurrentPages);
        var tasks = new List<Task>();

        // Thread-safe collections for successful documents and failures
        var successfulDocs = new ConcurrentBag<PresidentialAction>();
        var failureList = new ConcurrentBag<ScrapingError>();

        foreach (var document in documents)
        {
            await semaphore.WaitAsync(cancellationToken);

            // Create a new task for each URL with new thread
            var task = Task.Run(async () =>
            {
                var page = await browserService.GetPageAsync();
                try
                {
                    var (action, errorMessage) = await ScrapeDocumentAsync(page, document);
                    if (action is not null)
                        successfulDocs.Add(action);
                    else if (errorMessage is not null)
                        failureList.Add(new ScrapingError(document, errorMessage));
                }
                catch (Exception ex)
                {
                    failureList.Add(new ScrapingError(document, ex.Message));
                }
                finally
                {
                    await browserService.ClosePageAsync(page);
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        // Wait for all scraping tasks to finish
        await Task.WhenAll(tasks);

        // Transfer results to scraping result object
        result.Successful.AddRange(successfulDocs);
        foreach (var failure in failureList) result.Failures.Add(failure);

        return result;
    }

    // Scrape a single document and return either a populated entity or error message
    private async Task<(PresidentialAction? Action, string? ErrorMessage)> ScrapeDocumentAsync(IPage page,
        DocumentInfo document)
    {
        var response =
            await page.GotoAsync(document.Url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (response is not { Ok: true }) return (null, "Failed to load page");

        var category = await ExtractTextAsync(page, CategorySelector);
        var contentHtml = await ExtractContentAsync(page);
        if (string.IsNullOrEmpty(contentHtml)) return (null, "Failed to extract content");

        var categoryType = ParseCategoryType(category, document.Title, contentHtml);
        if (categoryType is null) return (null, $"Failed to extract or infer category for {document.Url}");

        var publicationDate = await ExtractDateTimeInUtcAsync(page);
        if (publicationDate is null) return (null, "Failed to extract publication date");

        return (new PresidentialAction
        {
            SubCategoryId = (int)categoryType.Value,
            Title = document.Title,
            Content = markdownConverter.Convert(contentHtml),
            SourceUrl = document.Url,
            PublishedAt = publicationDate.Value,
            TranslationStatus = TranslationStatus.Pending
        }, null);
    }

    // Extract text content from a page element by selector
    private static async Task<string?> ExtractTextAsync(IPage page, string selector)
    {
        var element = page.Locator(selector);
        if (await element.CountAsync() == 0)
            return null;

        var text = await element.InnerTextAsync();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    // Determine document subcategory type based on content, title, or category text
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

        // Fallback inference based on content keywords
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

    // Extract and combine paragraph content from the page’s main content area
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

    // Extract the publication date in UTC format from the page
    private static async Task<DateTime?> ExtractDateTimeInUtcAsync(IPage page)
    {
        var element = page.Locator(DateSelector);

        var dateAttribute = await element.GetAttributeAsync("datetime");
        if (!string.IsNullOrWhiteSpace(dateAttribute) && DateTime.TryParse(dateAttribute, out var parsedDate))
            return parsedDate.ToUniversalTime();

        return null;
    }
}