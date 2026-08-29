using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RideSharing.IntegrationTests;

public class HealthControllerTests : IClassFixture<IntegrationTestFixture>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthControllerTests(IntegrationTestFixture fixture, WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = fixture.SqlConnectionString,
                    ["RabbitMq:Host"] = fixture.RabbitMqHost,
                    ["Jwt:Key"] = "IntegrationTestKeyThatIsAtLeast32Chars!!",
                    ["Redis:ConnectionString"] = "localhost:6379"
                });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
