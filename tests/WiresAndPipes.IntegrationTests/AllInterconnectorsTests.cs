using Microsoft.Extensions.DependencyInjection;
using WiresAndPipes.Api.Polling;
using Xunit;

namespace WiresAndPipes.IntegrationTests;

public sealed class AllInterconnectorsTests : IClassFixture<ElectricityApiFactory>
{
    private readonly ElectricityApiFactory _factory;

    public AllInterconnectorsTests(ElectricityApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LatestEndpoint_ReturnsEveryInterconnector_AndExcludesDomesticFuelTypes()
    {
        using var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var pollCycle = scope.ServiceProvider.GetRequiredService<IElexonPollCycle>();
            await pollCycle.RunOnceAsync(CancellationToken.None);
        }

        var readings = await HttpPolling.PollUntilAsync<List<LatestReadingResponse>>(
            client,
            "/api/interconnectors/latest",
            list => list.Count >= 2,
            TimeSpan.FromSeconds(30));

        var codes = readings.Select(r => r.InterconnectorCode).ToList();
        Assert.Contains(StubElexonClient.PrimaryInterconnectorCode, codes);
        Assert.Contains(StubElexonClient.SecondaryInterconnectorCode, codes);
        Assert.DoesNotContain(StubElexonClient.DomesticFuelType, codes);

        var secondary = readings.Single(r => r.InterconnectorCode == StubElexonClient.SecondaryInterconnectorCode);
        Assert.Equal(StubElexonClient.SecondaryInterconnectorGenerationMw, secondary.GenerationMw);
    }
}
