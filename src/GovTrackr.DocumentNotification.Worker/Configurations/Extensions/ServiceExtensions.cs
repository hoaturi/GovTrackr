using GovTrackr.DocumentNotification.Worker.Configurations.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.DocumentNotification.Worker.Configurations.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMassTransitWithAzureServiceBus();

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
}