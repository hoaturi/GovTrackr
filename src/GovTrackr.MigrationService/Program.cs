using GovTrackr.MigrationService;
using GovTrackr.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Shared.Infrastructure.Persistence.Context;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<DbInitializer>();

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GovTrackr"), pgOptions =>
    {
        pgOptions.MigrationsAssembly(builder.Environment.ApplicationName);
        pgOptions.ExecutionStrategy(c => new NpgsqlRetryingExecutionStrategy(c));
    })
);

builder.EnrichNpgsqlDbContext<AppDbContext>(settings =>
    settings.DisableRetry = true
);

var host = builder.Build();

host.Run();