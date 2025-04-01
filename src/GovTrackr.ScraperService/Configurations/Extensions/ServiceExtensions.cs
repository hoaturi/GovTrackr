using GovTrackr.ScraperService.Configurations.Options;
using GovTrackr.ScraperService.Contracts.Html;
using GovTrackr.ScraperService.Contracts.Scraping;
using GovTrackr.ScraperService.Services.Html;
using GovTrackr.ScraperService.Services.Scraping;
using GovTrackr.ScraperService.Services.Scraping.Scrapers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace GovTrackr.ScraperService.Configurations.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMassTransit()
            // .AddScrapingService()
            .AddPresidentialActionScraper()
            .AddHostedService<DocumentScrapingService>()
            .AddHtmlToMarkdownConverter()
            .AddPlaywright();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<ConnectionStringsOption>()
            .Bind(configuration.GetSection(ConnectionStringsOption.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    private static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionStringsOptions =
                serviceProvider.GetRequiredService<IOptions<ConnectionStringsOption>>().Value;
            options.UseNpgsql(connectionStringsOptions.GovTrackrDb);
        });

        return services;
    }

    private static IServiceCollection AddMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionStringsOptions = context.GetRequiredService<IOptions<ConnectionStringsOption>>().Value;
                cfg.Host(connectionStringsOptions.AzureServiceBus);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddScrapingService(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentScrapingService, DocumentScrapingService>();
        return services;
    }

    private static IServiceCollection AddPresidentialActionScraper(this IServiceCollection services)
    {
        services.AddKeyedScoped<IScraper, PresidentialActionScraper>(DocumentCategoryType.PresidentialAction);

        return services;
    }

    private static IServiceCollection AddHtmlToMarkdownConverter(this IServiceCollection services)
    {
        services.AddSingleton<IHtmlConverter, HtmlToMarkdownConverter>();

        return services;
    }

    private static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IBrowserService, PlaywrightBrowserService>();

        return services;
    }
}