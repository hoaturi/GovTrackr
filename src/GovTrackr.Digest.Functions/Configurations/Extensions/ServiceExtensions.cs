using Amazon.SimpleEmailV2;
using GovTrackr.Digest.Functions.Application.Interfaces;
using GovTrackr.Digest.Functions.Configurations.Options;
using GovTrackr.Digest.Functions.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Digest.Functions.Configurations.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddEmailService();
            .AddDigestContentBuilder();

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

    private static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        services.AddSingleton<AmazonSimpleEmailServiceV2Client>(sp =>
        {
            var awsOptions = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
            return new AmazonSimpleEmailServiceV2Client(awsOptions.AccessKey, awsOptions.SecretKey);
        });

        services.AddSingleton<IEmailService, EmailService>();


    private static IServiceCollection AddDigestContentBuilder(this IServiceCollection services)
    {
        services.AddScoped<IDigestContentBuilder, DigestContentBuilder>();

        return services;
    }
        return services;
    }
}