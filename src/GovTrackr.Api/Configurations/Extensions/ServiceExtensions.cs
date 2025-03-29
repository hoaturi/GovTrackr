using GovTrackr.Application.Configurations.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence.Context;

namespace GovTrackr.Application.Configurations.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddConfigOptions(configuration)
            // .AddDatabaseService()
            .AddMediatrService();

        return services;
    }

    private static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithValidateOnStart<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    private static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(dbOptions.ConnectionString);
        });

        return services;
    }

    private static IServiceCollection AddMediatrService(this IServiceCollection services)
    {
        services.AddMediatR(config => { config.RegisterServicesFromAssemblyContaining<Program>(); });

        return services;
    }
}