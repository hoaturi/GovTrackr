using GovTrackr.DocumentDiscovery.Functions.Application.Interfaces;
using GovTrackr.DocumentDiscovery.Functions.Configurations.Options;
using GovTrackr.DocumentDiscovery.Functions.Infrastructure.Strategies;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Browser;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentDiscovery.Functions.Configurations.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMassTransitWithAzureServiceBus()
            .AddPlaywright()
            .AddDiscoveryStrategies();

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

    private static IServiceCollection AddMassTransitWithAzureServiceBus(this IServiceCollection services)
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

    private static IServiceCollection AddPlaywright(this IServiceCollection services)
    {
        services.AddSingleton<IBrowserService, PlaywrightService>();

        return services;
    }

    private static IServiceCollection AddDiscoveryStrategies(this IServiceCollection services)
    {
        services.AddScoped<IDocumentDiscoveryStrategy, PresidentialActionStrategy>();

        return services;
    }
}