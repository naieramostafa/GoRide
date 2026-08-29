using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace RideSharing.IntegrationTests;

public class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3-management").Build();

    public string SqlConnectionString => _sqlContainer.GetConnectionString();
    public string RabbitMqHost => _rabbitMqContainer.Hostname;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }
}
