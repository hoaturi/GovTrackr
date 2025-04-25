using GovTrackr.Api.Configurations.Extensions;
using GovTrackr.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();

app.UseAuthorization();

app.UsePathBase("/api");
app.MapControllers();

app.UseExceptionHandler();

app.UseCors();

app.Run();