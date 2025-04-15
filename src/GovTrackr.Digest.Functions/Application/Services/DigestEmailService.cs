using GovTrackr.Digest.Functions.Application.Dtos;
using GovTrackr.Digest.Functions.Application.Interfaces;
using GovTrackr.Digest.Functions.Configurations.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Domain.Subscription;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Digest.Functions.Application.Services;

public class DigestEmailService(
    AppDbContext dbContext,
    IDigestEmailBuilder emailBuilder,
    IEmailService emailService,
    IOptions<EmailOptions> emailOptions,
    ILogger<DigestEmailService> logger)
    : IDigestEmailService
{
    private const int DigestPeriodDays = 7;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public async Task SendDigestEmailAsync(CancellationToken cancellationToken)
    {
        var subscriptions = await GetSubscriptionsAsync(cancellationToken);
        if (subscriptions.Count == 0) return;

        var dateRange = GetDigestDateRange();
        var presidentialActions =
            await GetPresidentialActionsAsync(dateRange.startDate, dateRange.endDate, cancellationToken);
        if (presidentialActions.Count == 0) return;

        var emailSubject =
            $"📰 주간 미 정부 발표 요약 다이제스트 ({dateRange.startDate:yyyy-MM-dd} ~ {dateRange.endDate:yyyy-MM-dd})";
        var emailContent = await BuildEmailTemplateAsync(presidentialActions, dateRange, cancellationToken);
        await ProcessDigestEmailsAsync(subscriptions, emailSubject, emailContent, cancellationToken);
    }

    private async Task<string> BuildEmailTemplateAsync(
        List<PresidentialActionDto> presidentialActions,
        (DateTime startDate, DateTime endDate) dateRange,
        CancellationToken cancellationToken)
    {
        var dto = new DigestEmailTemplateDto(presidentialActions, dateRange.startDate, dateRange.endDate);
        return await emailBuilder.GetDigestContentAsync(dto, cancellationToken);
    }

    private async Task ProcessDigestEmailsAsync(
        List<SubscriptionDto> subscriptions,
        string emailSubject,
        string emailTemplate,
        CancellationToken cancellationToken)
    {
        var failedCount = 0;

        foreach (var subscription in subscriptions)
        {
            var unsubscribeUrl = $"{_emailOptions.BaseUrl}/unsubscribe/{subscription.Id}";
            var personalizedTemplate = emailBuilder.InjectUnsubscribeLink(emailTemplate, unsubscribeUrl);

            var result =
                await emailService.SendEmailAsync(subscription.Email, emailSubject, personalizedTemplate,
                    cancellationToken);

            if (!result) failedCount++;
        }

        if (failedCount > 0)
            logger.LogWarning("Digest sent with {FailedCount} failures out of {TotalCount} subscribers.", failedCount,
                subscriptions.Count);
        else
            logger.LogInformation("Digest sent successfully to all {TotalCount} subscribers.", subscriptions.Count);
    }

    private async Task<List<SubscriptionDto>> GetSubscriptionsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.DigestSubscriptions
            .Where(x => x.Status == DigestSubscriptionStatus.Active
            )
            .Select(x => new SubscriptionDto(x.Id, x.Email, x.UnsubscribeToken))
            .ToListAsync(cancellationToken);
    }

    private static (DateTime startDate, DateTime endDate) GetDigestDateRange()
    {
        var today = DateTime.UtcNow.Date;
        return (startDate: today.AddDays(-DigestPeriodDays), endDate: today);
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