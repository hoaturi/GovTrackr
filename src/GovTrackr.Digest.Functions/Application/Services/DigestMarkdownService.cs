using GovTrackr.Digest.Functions.Application.Dtos;
using GovTrackr.Digest.Functions.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Digest;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Digest.Functions.Application.Services;

public class DigestMarkdownService(AppDbContext dbContext, IDigestMarkdownBuilder markdownBuilder)
    : IDigestMarkdownService
{
    public async Task CreateDigestAsync(CancellationToken cancellationToken)
    {
        var dateRange = GetDigestDateRange();

        if (await DigestAlreadyExistsAsync(dateRange, cancellationToken))
            return;

        var presidentialActions =
            await GetPresidentialActionsAsync(dateRange.startDate, dateRange.endDate, cancellationToken);
        var markdown = markdownBuilder.BuildMarkdown(presidentialActions, dateRange.startDate);

        await SaveDigestAsync(markdown, dateRange, cancellationToken);
    }

    private async Task<bool> DigestAlreadyExistsAsync(
        (DateTime startDate, DateTime endDate) dateRange,
        CancellationToken cancellationToken)
    {
        // Uses only the date part
        return await dbContext.Digests
            .AnyAsync(d => d.StartDate == dateRange.startDate.Date &&
                           d.EndDate == dateRange.endDate.Date &&
                           d.Interval == DigestInterval.Weekly,
                cancellationToken);
    }

    private static (DateTime startDate, DateTime endDate) GetDigestDateRange()
    {
        var today = DateTime.UtcNow.Date;
        return (
            startDate: today.AddDays(-7),
            endDate: today
        );
    }

    private async Task SaveDigestAsync(
        string content,
        (DateTime startDate, DateTime endDate) dateRange,
        CancellationToken cancellationToken)
    {
        var digest = new Shared.Domain.Digest.Digest
        {
            StartDate = dateRange.startDate.Date, // Ensure the date is stored without time
            EndDate = dateRange.endDate.Date,
            Content = content,
            Interval = DigestInterval.Weekly
        };

        dbContext.Add(digest);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<PresidentialActionDto>> GetPresidentialActionsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        return await dbContext.PresidentialActionTranslations
            .Where(x => x.PresidentialAction.PublishedAt >= startDate
                        && x.PresidentialAction.PublishedAt <= endDate)
            .OrderByDescending(x => x.PresidentialAction.PublishedAt)
            .Select(x => new PresidentialActionDto(
                x.Id,
                x.Title,
                x.Summary,
                x.PresidentialAction.SourceUrl,
                x.PresidentialAction.PublishedAt
            ))
            .ToListAsync(cancellationToken);
    }
}