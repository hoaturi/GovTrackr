using System;
using System.Threading.Tasks;
using Bogus;
using GovTrackr.Application.Domain.Common;
using GovTrackr.Application.Domain.PresidentialAction;
using GovTrackr.Application.Infrastructure.Persistence.Context;

namespace GovTrackr.WebApp.Tests;

/// <summary>
///     Helper class for creating test data in integration tests
/// </summary>
public class TestDataHelper(AppDbContext dbContext)
{
    public async Task CreatePresidentialActionsAsync(int count,
        DocumentSubCategoryType subCategoryType,
        DateTime fromDate, DateTime toDate)
    {
        var faker = new Faker<PresidentialAction>()
            .RuleFor(a => a.Id, f => Guid.NewGuid())
            .RuleFor(a => a.Title, f => f.Lorem.Sentence(5))
            .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(2))
            .RuleFor(a => a.SourceUrl, f => f.Internet.Url())
            .RuleFor(a => a.PublishedAt, f => f.Date.Between(fromDate, toDate))
            .RuleFor(a => a.SubCategoryId, (int)subCategoryType)
            .RuleFor(a => a.TranslationStatus, TranslationStatus.Completed);

        var actions = faker.Generate(count);
        var classification = await dbContext.DocumentSubCategories.FindAsync((int)subCategoryType);

        foreach (var action in actions)
        {
            action.SubCategory = classification!;

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
    }

    public async Task CreatePresidentialActionWithTranslationAsync(
        Guid? translationId = null,
        DocumentSubCategoryType subCategoryType = DocumentSubCategoryType.ExecutiveOrder,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var actualTranslationId = translationId ?? Guid.NewGuid();
        var actionId = Guid.NewGuid();

        var actualFromDate = fromDate ?? DateTime.UtcNow.AddDays(-10);
        var actualToDate = toDate ?? DateTime.UtcNow;

        var actionFaker = new Faker<PresidentialAction>()
            .RuleFor(a => a.Id, f => actionId)
            .RuleFor(a => a.Title, f => f.Lorem.Sentence(5))
            .RuleFor(a => a.Content, f => f.Lorem.Paragraphs(2))
            .RuleFor(a => a.SourceUrl, f => f.Internet.Url())
            .RuleFor(a => a.PublishedAt, f => f.Date.Between(actualFromDate, actualToDate))
            .RuleFor(a => a.SubCategoryId, (int)subCategoryType)
            .RuleFor(a => a.TranslationStatus, TranslationStatus.Completed);

        var action = actionFaker.Generate();

        var classification = await dbContext.DocumentSubCategories.FindAsync((int)subCategoryType);
        action.SubCategory = classification!;

        var translation = new PresidentialActionTranslation
        {
            Id = actualTranslationId,
            PresidentialActionId = action.Id,
            Title = $"Translated: {action.Title}",
            Content = $"Translated content for {action.Title}\n\n{action.Content}",
            Summary = $"Summary of {action.Title}",
            PresidentialAction = action
        };

        await dbContext.PresidentialActions.AddAsync(action);
        await dbContext.PresidentialActionTranslations.AddAsync(translation);
        await dbContext.SaveChangesAsync();
    }
}