using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using GovTrackr.Notification.Functions.Application.Interfaces;
using GovTrackr.Notification.Functions.Application.Services;
using GovTrackr.Notification.Functions.Configurations.Options;
using GovTrackr.Notification.Functions.Consumers;
using GovTrackr.Notification.Functions.Infrastructure.Firebase;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Domain.Common;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Notification.Functions.Configurations.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMassTransitWithAzureServiceBus()
            .AddNotificationServices()
            .AddNotificationServices();

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
            config.AddConsumer<DocumentTranslatedConsumer>();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionStringsOptions = context.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
                cfg.Host(connectionStringsOptions.AzureServiceBus);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        var credentialPath = Path.Combine(AppContext.BaseDirectory, "govtrackr-dev-adminsdk.json");

        if (!File.Exists(credentialPath))
            throw new FileNotFoundException("Firebase credential file not found.", credentialPath);

        if (FirebaseApp.DefaultInstance is null)
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath)
            });

        services.AddSingleton(FirebaseMessaging.DefaultInstance);
        services.AddSingleton<IPushService, FcmService>();

        services.AddKeyedScoped<INotificationService, PresidentialNotificationService>(DocumentCategoryType
            .PresidentialAction);

        return services;
    }
}