using GovTrackr.DocumentScraping.Worker.Application.Interfaces;
using GovTrackr.DocumentScraping.Worker.Configurations.Options;
using GovTrackr.DocumentScraping.Worker.Consumers;
using GovTrackr.DocumentScraping.Worker.Infrastructure.Converters;
using GovTrackr.DocumentScraping.Worker.Infrastructure.Scrapers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Browser;
using Shared.Domain.Common;
using Shared.Infrastructure.Browser;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentScraping.Worker.Configurations.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMassTransitWithConsumer()
            .AddPresidentialActionScraper()
            .AddHtmlToMarkdownConverter()
            .AddPlaywright();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<ScrapersOptions>()
            .Bind(configuration.GetSection(ScrapersOptions.SectionName))
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

    private static IServiceCollection AddMassTransitWithConsumer(this IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.AddConsumer<DocumentDiscoveredConsumer>();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionStringsOptions = context.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
                cfg.Host(connectionStringsOptions.AzureServiceBus);
                cfg.ConfigureEndpoints(context);
            });
        });

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
        services.AddSingleton<IBrowserService, PlaywrightService>();

        return services;
    }
}