using System.Net;
using System.Text;
using FluentResults;
using GovTrackr.DocumentScraping.Worker.Application.Dtos;
using GovTrackr.DocumentScraping.Worker.Application.Errors;
using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using Microsoft.Playwright;
using Shared.Abstractions.Browser;
using Shared.Domain.Common;
using Shared.MessageContracts;

namespace GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers;

internal class PresidentialActionScraper(
    IHtmlConverter markdownConverter,
    IBrowserService browserService
) : IScraper
{
    private const string CategorySelector = ".wp-block-whitehouse-byline-subcategory__link";
    private const string DateSelector = ".wp-block-post-date time";
    private const string ContentContainerSelector = ".entry-content.wp-block-post-content";

    public async Task<Result<ScrapedPresidentialActionDto>> ScrapeAsync(DocumentInfo document,
        CancellationToken cancellationToken)
    {
        var page = await browserService.GetPageAsync();

        try
        {
            var result = await NavigateToPageAsync(page, document.Url);

            if (result.IsFailed) return Result.Fail(result.Errors.First());

            return await ParseAsync(page, document);
        }
        finally
        {
            await browserService.ClosePageAsync(page);
        }
    }

    private static async Task<Result<IPage>> NavigateToPageAsync(
        IPage page,
        string url)
    {
        var response = await page.GotoAsync(url,
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        if (response is null)
            throw ScrapingExceptions.NullResponse;

        if (response.Ok)
            return Result.Ok(page);

        var status = response.Status;

        if (status is (int)HttpStatusCode.TooManyRequests or >= (int)HttpStatusCode.InternalServerError)
            throw ScrapingExceptions.TransientHttpError(status);

        return Result.Fail(ScrapingErrors.ApiFailed(status));
    }

    private async Task<Result<ScrapedPresidentialActionDto>> ParseAsync(IPage page, DocumentInfo document)
    {
        var category = await ExtractTextAsync(page, CategorySelector);
        var contentHtml = await ExtractContentAsync(page);
        if (string.IsNullOrEmpty(contentHtml))
            return Result.Fail(ScrapingErrors.EmptyContent);

        var categoryType = ParseCategoryType(category, document.Title, contentHtml);
        if (categoryType is null)
            return Result.Fail(ScrapingErrors.FailedToParseCategory);

        var publicationDate = await ExtractDateTimeInUtcAsync(page);
        if (publicationDate is null)
            return Result.Fail(ScrapingErrors.FailedToParseDate);

        var dto = new ScrapedPresidentialActionDto(
            document.Title,
            markdownConverter.Convert(contentHtml),
            document.Url,
            publicationDate.Value,
            categoryType.Value
        );

        return Result.Ok(dto);
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
        {
            var html = await paragraphs.Nth(i).InnerHTMLAsync();
            html = html
                .Replace("<br>", "<br>\n")
                .Replace("<br/>", "<br/>\n")
                .Replace("<br />", "<br />\n")
                .TrimStart();

            builder.Append(html).Append("\n\n");
        }

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