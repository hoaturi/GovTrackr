using GovTrackr.DocumentScraping.Worker.Configurations.Extensions;
using GovTrackr.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAppServices(builder.Configuration);

var host = builder.Build();

host.Run();