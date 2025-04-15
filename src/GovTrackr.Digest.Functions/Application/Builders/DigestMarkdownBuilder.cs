using System.Text;
using GovTrackr.Digest.Functions.Application.Dtos;
using GovTrackr.Digest.Functions.Application.Interfaces;
using GovTrackr.Digest.Functions.Configurations.Options;
using Microsoft.Extensions.Options;

namespace GovTrackr.Digest.Functions.Application.Builders;

public class DigestMarkdownBuilder(IOptions<EmailOptions> emailOptions) : IDigestMarkdownBuilder
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public string BuildMarkdown(List<PresidentialActionDto> presidentialActions, DateTime startDate)
    {
        var sb = new StringBuilder();
        var today = DateTime.UtcNow;

        AppendHeader(sb, startDate, today);
        AppendDocuments(sb, presidentialActions);

        return sb.ToString();
    }

    private static void AppendHeader(StringBuilder sb, DateTime startDate, DateTime today)
    {
        sb.AppendLine($"# 📰 주간 미 정부 발표 요약 다이제스트 ({startDate:yyyy-M-d dddd} - {today:yyyy-M-d dddd})");
        sb.AppendLine();
        sb.AppendLine("이번 주에 번역 및 요약된 미 정부 발표에 대한 주요 내용입니다. 각 제목을 클릭하시면 상세 요약 페이지로 이동합니다.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
    }

    private void AppendDocuments(StringBuilder sb, List<PresidentialActionDto> presidentialActions)
    {
        foreach (var action in presidentialActions)
        {
            var detailedSummaryUrl = GenerateDetailedSummaryUrl(action);

            sb.AppendLine($"### 📄 [{action.Title}]({detailedSummaryUrl})");
            sb.AppendLine();
            sb.AppendLine($"* **📅 발행일:** {action.PublishedAt:yyyy-MM-dd} (현지 시간)");
            sb.AppendLine($"* **🔗 원문 출처:** [원본 문서 링크]({action.SourceUrl})");

            var summaryText = string.IsNullOrWhiteSpace(action.Summary)
                ? "요약 정보가 없습니다."
                : action.Summary;

            sb.AppendLine($" > {summaryText.Replace("\n", "\n > ")}");
            sb.AppendLine();
            sb.AppendLine("---");
        }
    }

    private string GenerateDetailedSummaryUrl(PresidentialActionDto action)
    {
        return $"{_emailOptions.BaseUrl}/presidential-actions/{action.Id}";
    }
}