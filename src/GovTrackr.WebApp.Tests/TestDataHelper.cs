using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using GovTrackr.Application.Domain.PresidentialAction;
using GovTrackr.Application.Infrastructure.Persistence.Context;

namespace GovTrackr.WebApp.Tests;

/// <summary>
///     Helper class for creating test data in integration tests
/// </summary>
public class TestDataHelper(AppDbContext dbContext)
{
    public async Task<List<Guid>> CreatePresidentialActionsWithDateAsync(int count, int classificationType,
        DateTime fromDate, DateTime toDate)
    {
        var titlePrefix = classificationType switch
        {
            1 => "Executive Order",
            2 => "Proclamation",
            3 => "Presidential Memorandum",
            _ => throw new ArgumentOutOfRangeException(nameof(classificationType), classificationType, null)
        };

        var faker = new Faker<PresidentialAction>()
            .RuleFor(a => a.Id, f => Guid.NewGuid())
            .RuleFor(a => a.Title, f => $"{titlePrefix}: {f.Lorem.Sentence(5)}")
            .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(2))
            .RuleFor(a => a.SourceUrl, f => f.Internet.Url())
            .RuleFor(a => a.PublishedAt, f => f.Date.Between(fromDate, toDate))
            .RuleFor(a => a.ClassificationId, classificationType)
            .RuleFor(a => a.TranslationStatus, TranslationStatus.Completed);

        var actions = faker.Generate(count);
        var classification = await dbContext.DocumentClassifications.FindAsync(classificationType);

        foreach (var action in actions)
        {
            action.Classification = classification;

            var translation = new PresidentialActionTranslation
            {
                Id = Guid.NewGuid(),
                PresidentialActionId = action.Id,
                Title = $"Translated: {action.Title}",
                Content = $"Translated content for {action.Title}\n\n{action.Content}",
                Summary = $"Summary of {action.Title}",
                PresidentialAction = action
            };

            await dbContext.PresidentialActions.AddAsync(action);
            await dbContext.PresidentialActionTranslations.AddAsync(translation);
        }

        await dbContext.SaveChangesAsync();

        return actions.Select(a => a.Id).ToList();
    }
}