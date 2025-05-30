using GovTrackr.MigrationService;
using GovTrackr.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Persistence.Context;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
    .Bind(builder.Configuration.GetSection(ConnectionStringsOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
    options.UseNpgsql(dbOptions.GovTrackrDb);
});

builder.Services.AddHostedService<DbInitializer>();

var host = builder.Build();

host.Run();