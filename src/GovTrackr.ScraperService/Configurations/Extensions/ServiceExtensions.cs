using GovTrackr.ScraperService.Abstractions.HtmlProcessing;
using GovTrackr.ScraperService.Abstractions.Scraping;
using GovTrackr.ScraperService.Configurations.Options;
using GovTrackr.ScraperService.Scraping;
using GovTrackr.ScraperService.Scraping.Scrapers;
using GovTrackr.ScraperService.Services.HtmlProcessing;
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
            .AddHostedService<ScrapingService>()
            .AddHtmlToMarkdownConverter()
            .AddPlaywright();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    private static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionStringsOptions =
                serviceProvider.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
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
                var connectionStringsOptions = context.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
                cfg.Host(connectionStringsOptions.AzureServiceBus);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddScrapingService(this IServiceCollection services)
    {
        services.AddSingleton<IScrapingService, ScrapingService>();
        return services;
    }

    private static IServiceCollection AddPresidentialActionScraper(this IServiceCollection services)
    {
        services.AddKeyedScoped<IScraper, PresidentialActionScraper>(DocumentCategoryType.PresidentialAction);

        return services;
    }

    private static IServiceCollection AddHtmlToMarkdownConverter(this IServiceCollection services)
    {
        services.AddSingleton<IHtmlToMarkdownConverter, HtmlToMarkdownConverter>();

        return services;
    }

    private static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IPlaywrightService, PlaywrightService>();

        return services;
    }
}