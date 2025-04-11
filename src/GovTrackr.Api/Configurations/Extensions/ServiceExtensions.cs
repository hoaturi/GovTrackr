using GovTrackr.Application.Configurations.Options;
using GovTrackr.Api.Configurations.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Api.Configurations.Extensions;

internal static class ServiceExtensions
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            .AddDatabaseService()
            .AddMediatrService();

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
            var dbOptions = serviceProvider.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
            options.UseNpgsql(dbOptions.GovTrackrDb);
        });

        return services;
    }

    private static IServiceCollection AddMediatrService(this IServiceCollection services)
    {
        services.AddMediatR(config => { config.RegisterServicesFromAssemblyContaining<Program>(); });

        return services;
    }
}