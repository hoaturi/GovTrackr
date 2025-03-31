using GovTrackr.ScraperService;
using GovTrackr.ScraperService.Configurations.Extensions;
using GovTrackr.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();