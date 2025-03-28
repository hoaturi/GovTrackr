using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using GovTrackr.Application.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace GovTrackr.WebApp.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithEnvironment("POSTGRES_USER", "postgres")
        .WithEnvironment("POSTGRES_PASSWORD", "password")
        .WithEnvironment("POSTGRES_DB", "govtrackr_test")
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    public AppDbContext DbContext { get; private set; }
    public string ConnectionString { get; private set; }


    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var mappedPort = _postgreSqlContainer.GetMappedPublicPort(5432);

        ConnectionString =
            $"Host=localhost;Port={mappedPort};Database=govtrackr_test;Username=postgres;Password=password";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        DbContext = new AppDbContext(options);

        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.StopAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}