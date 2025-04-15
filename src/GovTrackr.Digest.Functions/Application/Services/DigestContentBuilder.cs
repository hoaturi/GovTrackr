using System.Text;
using GovTrackr.Digest.Functions.Application.Dtos;
using GovTrackr.Digest.Functions.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Digest;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Digest.Functions.Application.Services;

public class DigestContentBuilder(AppDbContext dbContext) : IDigestContentBuilder
{
    public async Task<string?> BuildMarkdownDigestContentAsync(CancellationToken cancellationToken)
    {
        var dateRange = GetDigestDateRange();

        if (await DigestAlreadyExistsAsync(dateRange, cancellationToken))
            return null;

        var presidentialActions =
            await GetPresidentialActionsAsync(dateRange.startDate, dateRange.endDate, cancellationToken);
        var markdown = GenerateMarkdownDigest(presidentialActions, dateRange.startDate);

        await SaveDigestAsync(markdown, dateRange, cancellationToken);
        return markdown;
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

    private static string GenerateMarkdownDigest(List<PresidentialActionDto> presidentialActions, DateTime startDate)
    {
        var sb = new StringBuilder();
        var today = DateTime.UtcNow;

        AppendHeader(sb, startDate, today);

        if (presidentialActions.Count == 0)
            AppendNoDocumentsMessage(sb);
        else
            AppendDocuments(sb, presidentialActions);

        return sb.ToString();
    }

    private static void AppendHeader(StringBuilder sb, DateTime startDate, DateTime today)
    {
        sb.AppendLine($"# 📰 주간 정부 문서 번역 다이제스트 ({startDate:yyyy-M-d dddd} - {today:yyyy-M-d dddd})");
        sb.AppendLine();
        sb.AppendLine("이번 주에 번역 및 요약된 주요 문서입니다. 각 제목을 클릭하시면 상세 요약 페이지로 이동합니다.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
    }

    private static void AppendNoDocumentsMessage(StringBuilder sb)
    {
        sb.AppendLine("이번 주에는 새로 번역된 문서가 없습니다.");
        sb.AppendLine();
        sb.AppendLine("---");
    }

    private static void AppendDocuments(StringBuilder sb, List<PresidentialActionDto> presidentialActions)
    {
        foreach (var action in presidentialActions)
        {
            var detailedSummaryUrl = GenerateDetailedSummaryUrl(action);

            sb.AppendLine($"### 📄 [{action.Title}]({detailedSummaryUrl})");
            sb.AppendLine();
            sb.AppendLine($"* **📅 발행일:** {action.PublishedAt:yyyy-MM-dd}");
            sb.AppendLine($"* **🔗 원문 출처:** [원본 문서 링크]({action.SourceUrl})");
            sb.AppendLine("* **\ud83d\udcdd 요약:**");

            var summaryText = string.IsNullOrWhiteSpace(action.Summary)
                ? "요약 정보가 없습니다."
                : action.Summary;

            sb.AppendLine($" > {summaryText.Replace("\n", "\n > ")}");
            sb.AppendLine();
            sb.AppendLine("---");
        }
    }

    private static string GenerateDetailedSummaryUrl(PresidentialActionDto action)
    {
        // TODO: Change this to the actual Base URL
        return $"https://www.govtrackr.com/presidential-actions/{action.Id}";
    }

    private async Task<List<PresidentialActionDto>> GetPresidentialActionsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        return await dbContext.PresidentialActionTranslations
            .Where(x => x.PresidentialAction.PublishedAt >= startDate
                        && x.PresidentialAction.PublishedAt <= endDate)
            .OrderBy(x => x.PresidentialAction.PublishedAt)
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