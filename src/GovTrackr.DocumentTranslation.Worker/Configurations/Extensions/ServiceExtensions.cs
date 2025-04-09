using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using GovTrackr.DocumentTranslation.Worker.Application.Services;
using GovTrackr.DocumentTranslation.Worker.Configurations.Options;
using GovTrackr.DocumentTranslation.Worker.Consumers;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Prompting;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentTranslation.Worker.Configurations.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddHttpClients()
            .AddPromptProvider()
            .AddTranslators()
            .AddTranslationServices()
            .AddMassTransitWithConsumer();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<GeminiOptions>()
            .Bind(configuration.GetSection(GeminiOptions.SectionName))
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

    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("GeminiClient", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.BaseAddress =
                new Uri(
                    $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={options.ApiKey}");
        });

        return services;
    }

    private static IServiceCollection AddPromptProvider(this IServiceCollection services)
    {
        services.AddSingleton<IPromptProvider, PromptProvider>();

        return services;
    }

    private static IServiceCollection AddTranslators(this IServiceCollection services)
    {
        services.AddKeyedScoped<ITranslator, PresidentialActionTranslator>(DocumentCategoryType.PresidentialAction);

        return services;
    }

    private static IServiceCollection AddTranslationServices(this IServiceCollection services)
    {
        services.AddKeyedScoped<ITranslationService, PresidentialActionTranslationService>(DocumentCategoryType
            .PresidentialAction);

        return services;
    }

    private static IServiceCollection AddMassTransitWithConsumer(this IServiceCollection services)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.AddConsumer<DocumentScrapedConsumer>();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionStringsOptions = context.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
                cfg.Host(connectionStringsOptions.AzureServiceBus);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}