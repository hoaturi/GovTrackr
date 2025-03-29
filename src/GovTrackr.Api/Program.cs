using GovTrackr.Application.Configurations.Extensions;
using GovTrackr.ServiceDefaults;
using Shared.Infrastructure.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<AppDbContext>("govtrackr-db");

builder.Services.AddAppServices(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();