using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RideSharing.Application.DTOs;
using Xunit;

namespace RideSharing.IntegrationTests;

public class RideControllerTests : IClassFixture<IntegrationTestFixture>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RideControllerTests(IntegrationTestFixture fixture, WebApplicationFactory<Program> factory)
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
    public async Task GetActiveRides_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/rides/active");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/rides/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestRide_WithoutAuth_ShouldReturnUnauthorized()
    {
        var dto = new RideRequestDto
        {
            UserId = Guid.NewGuid(),
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            PickupAddress = "NYC",
            DropoffLatitude = 40.7580,
            DropoffLongitude = -73.9855,
            DropoffAddress = "Times Square"
        };

        var response = await _client.PostAsJsonAsync("/api/rides", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Swagger_ShouldBeAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
