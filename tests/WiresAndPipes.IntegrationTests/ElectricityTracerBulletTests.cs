using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WiresAndPipes.Api.Polling;
using Xunit;

namespace WiresAndPipes.IntegrationTests;

public sealed class ElectricityTracerBulletTests : IClassFixture<ElectricityTracerBulletFactory>
{
    private readonly ElectricityTracerBulletFactory _factory;

    public ElectricityTracerBulletTests(ElectricityTracerBulletFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LatestEndpoint_ReturnsReading_AfterPollerFetchesAndConsumerNormalises()
    {
        using var client = _factory.CreateClient();

        // Drive one poll cycle through the real poller -> MassTransit -> consumer -> Postgres
        // path, using the stubbed Elexon client wired in by the factory.
        using (var scope = _factory.Services.CreateScope())
        {
            var pollCycle = scope.ServiceProvider.GetRequiredService<IElexonPollCycle>();
            await pollCycle.RunOnceAsync(CancellationToken.None);
        }

        var reading = await PollUntilAvailableAsync(client, TimeSpan.FromSeconds(30));

        Assert.Equal(StubElexonClient.TrackedInterconnectorCode, reading.InterconnectorCode);
        Assert.Equal(StubElexonClient.TrackedInterconnectorGenerationMw, reading.GenerationMw);
        Assert.Equal(StubElexonClient.SettlementPeriod, reading.SettlementPeriod);
    }

    private static async Task<LatestReadingResponse> PollUntilAvailableAsync(HttpClient client, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync($"/api/interconnectors/{StubElexonClient.TrackedInterconnectorCode}/latest");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var reading = await response.Content.ReadFromJsonAsync<LatestReadingResponse>();
                Assert.NotNull(reading);
                return reading;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        Assert.Fail($"No reading became available within {timeout}.");
        throw new UnreachableException();
    }

    private sealed record LatestReadingResponse(
        string InterconnectorCode,
        DateOnly SettlementDate,
        int SettlementPeriod,
        decimal GenerationMw,
        DateTimeOffset RecordedAt);
}
