using GovTrackr.Digest.Functions.Configurations.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAppServices(builder.Configuration);

builder.Build().Run();