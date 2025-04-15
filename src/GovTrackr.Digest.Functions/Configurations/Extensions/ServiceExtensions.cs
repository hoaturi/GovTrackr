using Amazon;
using Amazon.SimpleEmailV2;
using GovTrackr.Digest.Functions.Application.Interfaces;
using GovTrackr.Digest.Functions.Application.Services;
using GovTrackr.Digest.Functions.Configurations.Options;
using GovTrackr.Digest.Functions.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mjml.Net;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Digest.Functions.Configurations.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddEmailService()
            .AddDigestContentBuilder()
            .AddDigestService()
            .AddDigestEmailBuilder()
            .AddMjmlService();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
            .Bind(configuration.GetSection(ConnectionStringsOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<AwsOptions>()
            .Bind(configuration.GetSection(AwsOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptionsWithValidateOnStart<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
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
            return new AmazonSimpleEmailServiceV2Client(awsOptions.AccessKey, awsOptions.SecretKey,
                RegionEndpoint.APNortheast2);
        });

        services.AddSingleton<IEmailService, EmailService>();

        return services;
    }

    private static IServiceCollection AddDigestContentBuilder(this IServiceCollection services)
    {
        services.AddScoped<IDigestContentBuilder, DigestContentBuilder>();

        return services;
    }

    private static IServiceCollection AddDigestService(this IServiceCollection services)
    {
        services.AddScoped<IDigestService, DigestService>();

        return services;
    }

    private static IServiceCollection AddDigestEmailBuilder(this IServiceCollection services)
    {
        services.AddSingleton<IDigestEmailBuilder, DigestEmailBuilder>();

        return services;
    }

    private static IServiceCollection AddMjmlService(this IServiceCollection services)
    {
        services.AddSingleton<IMjmlRenderer, MjmlRenderer>(_ => new MjmlRenderer());

        return services;
    }
}