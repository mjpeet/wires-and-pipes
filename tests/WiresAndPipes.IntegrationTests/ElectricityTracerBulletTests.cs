using Microsoft.Extensions.DependencyInjection;
using WiresAndPipes.Api.Polling;
using Xunit;

namespace WiresAndPipes.IntegrationTests;

public sealed class ElectricityTracerBulletTests : IClassFixture<ElectricityApiFactory>
{
    private readonly ElectricityApiFactory _factory;

    public ElectricityTracerBulletTests(ElectricityApiFactory factory)
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

        var reading = await HttpPolling.PollUntilAsync<LatestReadingResponse>(
            client,
            $"/api/interconnectors/{StubElexonClient.PrimaryInterconnectorCode}/latest",
            _ => true,
            TimeSpan.FromSeconds(30));

        Assert.Equal(StubElexonClient.PrimaryInterconnectorCode, reading.InterconnectorCode);
        Assert.Equal(StubElexonClient.PrimaryInterconnectorGenerationMw, reading.GenerationMw);
        Assert.Equal(StubElexonClient.SettlementPeriod, reading.SettlementPeriod);
    }
}
