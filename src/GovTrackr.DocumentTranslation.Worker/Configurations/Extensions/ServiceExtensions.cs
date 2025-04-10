using System.Net;
using GovTrackr.DocumentTranslation.Worker.Application.Interfaces;
using GovTrackr.DocumentTranslation.Worker.Application.Services;
using GovTrackr.DocumentTranslation.Worker.Configurations.Options;
using GovTrackr.DocumentTranslation.Worker.Consumers;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Prompting;
using GovTrackr.DocumentTranslation.Worker.Infrastructure.Translators;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
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
#pragma warning disable EXTEXP0001
        services.AddHttpClient("GeminiClient", (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
                client.BaseAddress = new Uri(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro-preview-03-25:generateContent?key={options.ApiKey}");
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            // HACK: This is a workaround for the issue with HttpClientFactory and Polly
            // It overrides the default resilience policy
            .RemoveAllResilienceHandlers()
            .AddResilienceHandler("CustomGeminiPolicy", config =>
            {
                config.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 1,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(2),
                    ShouldHandle = args => ValueTask.FromResult(
                        // Default transient HTTP errors OR Polly Timeout Exception
                        args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError || // 5xx
                        args.Outcome.Result?.StatusCode == HttpStatusCode.RequestTimeout || // 408
                        args.Outcome.Exception is TimeoutRejectedException || // Explicitly handle Polly Timeout
                        args.Outcome.Exception is HttpRequestException // Default exception handling
                    )
                });

                config.AddTimeout(TimeSpan.FromSeconds(60));

                config.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromMinutes(3),
                    BreakDuration = TimeSpan.FromSeconds(15),
                    FailureRatio = 0.8,
                    MinimumThroughput = 20
                });
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
                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(10)); });
            });
        });

        return services;
    }
}