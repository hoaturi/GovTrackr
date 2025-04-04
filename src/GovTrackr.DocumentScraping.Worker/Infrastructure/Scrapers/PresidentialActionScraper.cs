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
        if (documents.Count == 0) return result;

        using var semaphore = new SemaphoreSlim(options.Value.MaxConcurrentPages);
        var tasks = documents.Select(doc => ScrapeWithConcurrencyAsync(doc, semaphore, cancellationToken)).ToList();

        var results = await Task.WhenAll(tasks);
        foreach (var (action, error, url) in results)
            if (action is not null)
                result.Successful.Add(action);
            else if (error is not null)
                result.Failures.Add(new ScrapingError(url, error));

        return result;
    }

    private async Task<(PresidentialAction? Action, string? Error, string Url)> ScrapeWithConcurrencyAsync(
        DocumentInfo document,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        var page = await browserService.GetPageAsync();

        try
        {
            var (action, error) = await ScrapeDocumentAsync(page, document);
            return (action, error, document.Url);
        }
        catch (Exception ex)
        {
            return (null, ex.Message, document.Url);
        }
        finally
        {
            await browserService.ClosePageAsync(page);
            semaphore.Release();
        }
    }

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
        if (categoryType is null) return (null, "Failed to infer category");

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

    private static async Task<string?> ExtractTextAsync(IPage page, string selector)
    {
        var element = page.Locator(selector);
        if (await element.CountAsync() == 0) return null;
        var text = await element.InnerTextAsync();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static async Task<string?> ExtractContentAsync(IPage page)
    {
        var container = page.Locator(ContentContainerSelector);
        if (await container.CountAsync() == 0) return null;

        var paragraphs = container.Locator("p");
        var count = await paragraphs.CountAsync();
        if (count == 0) return null;

        var builder = new StringBuilder();
        for (var i = 0; i < count; i++)
            builder.Append(await paragraphs.Nth(i).InnerHTMLAsync());

        return builder.ToString().Trim();
    }

    private static async Task<DateTime?> ExtractDateTimeInUtcAsync(IPage page)
    {
        var element = page.Locator(DateSelector);
        var dateAttr = await element.GetAttributeAsync("datetime");
        return DateTime.TryParse(dateAttr, out var date) ? date.ToUniversalTime() : null;
    }

    private static DocumentSubCategoryType? ParseCategoryType(string? category, string title, string content)
    {
        if (!string.IsNullOrWhiteSpace(category))
            return category.Trim() switch
            {
                "Executive Orders" => DocumentSubCategoryType.ExecutiveOrder,
                "Presidential Memoranda" => DocumentSubCategoryType.Memoranda,
                "Proclamations" => DocumentSubCategoryType.Proclamation,
                "Nominations & Appointments" => DocumentSubCategoryType.Nomination,
                _ => null
            };

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
}