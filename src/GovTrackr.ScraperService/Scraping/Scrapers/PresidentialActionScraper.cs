using System.Text;
using GovTrackr.ScraperService.Abstractions.HtmlProcessing;
using GovTrackr.ScraperService.Abstractions.Scraping;
using GovTrackr.ScraperService.Scraping.Models;
using Microsoft.Playwright;
using Shared.Domain.Common;
using Shared.Domain.PresidentialAction;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.ScraperService.Scraping.Scrapers;

internal class PresidentialActionScraper(
    AppDbContext dbContext,
    ILogger<PresidentialActionScraper> logger,
    IHtmlToMarkdownConverter markdownConverter,
    IPlaywrightService playwrightService)
    : IScraper
{

    private const string TitleSelector = ".wp-block-whitehouse-topper__headline";
    private const string CategorySelector = ".wp-block-whitehouse-byline-subcategory__link";
    private const string DateSelector = ".wp-block-post-date time";
    private const string ContentContainerSelector = ".entry-content.wp-block-post-content";

    public async Task ScrapeAsync(List<string> urls, CancellationToken cancellationToken)
    {
        var result = new ScrapingBatchResult();

        var page = await playwrightService.GetPageAsync();
        try
        {
            foreach (var url in urls.TakeWhile(url => !cancellationToken.IsCancellationRequested))
            {
                var document = await ScrapeDocumentAsync(page, url, result);
                if (document is not null) result.Successful.Add(document);
            }
        }
        finally
        {
            await playwrightService.ClosePageAsync(page);

            if (result.Successful.Count != 0)
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
                    result.Failures.Select(f => new { f.Url, ErrorMessage = f.Message }));
        }
    }

    private async Task<PresidentialAction?> ScrapeDocumentAsync(IPage page, string url, ScrapingBatchResult result)
    {
        var response = await page.GotoAsync(url,
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        if (response is null || !response.Ok)
        {
            result.Failures.Add(new ScrapingError(url, "Failed to load page"));
            return null;
        }

        // Extract document metadata
        var title = await ExtractTextAsync(page, TitleSelector);
        if (string.IsNullOrEmpty(title))
        {
            result.Failures.Add(new ScrapingError(url, "Title not found"));
            return null;
        }

        var category = await ExtractTextAsync(page, CategorySelector);
        var categoryType = ParseCategoryType(category);
        if (categoryType is null)
        {
            result.Failures.Add(new ScrapingError(url, "Category not found or invalid"));
            return null;
        }

        var publicationDate = await ExtractDateTimeInUtcAsync(page, DateSelector);
        if (publicationDate is null)
        {
            result.Failures.Add(new ScrapingError(url, "Publication date not found"));
            return null;
        }

        // Extract and convert content
        var contentHtml = await ExtractContentAsync(page, ContentContainerSelector);
        if (string.IsNullOrEmpty(contentHtml))
        {
            result.Failures.Add(new ScrapingError(url, "Content not found"));
            return null;
        }

        var contentMarkdown = markdownConverter.Convert(contentHtml);
        if (string.IsNullOrWhiteSpace(contentMarkdown))
        {
            result.Failures.Add(new ScrapingError(url, "Content conversion to Markdown failed"));
            return null;
        }

        return new PresidentialAction
        {
            SubCategoryId = (int)categoryType.Value,
            Title = title,
            Content = contentMarkdown,
            SourceUrl = url,
            PublishedAt = publicationDate.Value,
            TranslationStatus = TranslationStatus.Pending
        };
    }

    private static async Task<string?> ExtractTextAsync(IPage page, string selector)
    {
        var element = page.Locator(selector);
        if (await element.CountAsync() == 0)
            return null;

        var text = await element.InnerTextAsync();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static DocumentSubCategoryType? ParseCategoryType(string? category)
    {
        return category switch
        {
            "Executive Orders" => DocumentSubCategoryType.ExecutiveOrder,
            "Presidential Memoranda" => DocumentSubCategoryType.Memoranda,
            "Proclamations" => DocumentSubCategoryType.Proclamation,
            _ => null
        };
    }

    private static async Task<string?> ExtractContentAsync(IPage page, string containerSelector)
    {
        var contentContainer = page.Locator(containerSelector);
        if (await contentContainer.CountAsync() == 0)
            return null;

        var contentParagraphs = contentContainer.Locator("p");
        var paragraphCount = await contentParagraphs.CountAsync();
        if (paragraphCount == 0)
            return null;

        var contentBuilder = new StringBuilder();
        for (var i = 0; i < paragraphCount; i++)
        {
            var paragraph = contentParagraphs.Nth(i);
            var paragraphHtml = await paragraph.InnerHTMLAsync();
            contentBuilder.Append(paragraphHtml);
        }

        return contentBuilder.ToString().Trim();
    }

    private static async Task<DateTime?> ExtractDateTimeInUtcAsync(IPage page, string selector)
    {
        var element = page.Locator(selector);
        if (await element.CountAsync() == 0)
            return null;

        var dateText = await element.InnerTextAsync();
        if (string.IsNullOrWhiteSpace(dateText))
            return null;

        if (DateTimeOffset.TryParse(dateText, out var parsedDate))
            return parsedDate.UtcDateTime;

        var dateAttribute = await element.GetAttributeAsync("datetime");
        if (!string.IsNullOrWhiteSpace(dateAttribute) &&
            DateTimeOffset.TryParse(dateAttribute, out parsedDate))
            return parsedDate.UtcDateTime;

        return null;
    }
}