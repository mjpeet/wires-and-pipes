using MassTransit;
using WiresAndPipes.Api.Elexon;

namespace WiresAndPipes.Api.Polling;

public sealed class ElexonPollCycle(
    IElexonClient elexonClient,
    IPublishEndpoint publishEndpoint,
    TimeProvider timeProvider)
    : IElexonPollCycle
{
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var rawJson = await elexonClient.GetFuelHhRawAsync(cancellationToken);

        await publishEndpoint.Publish(new FuelHhRawDataReceived
        {
            RawJson = rawJson,
            RetrievedAt = timeProvider.GetUtcNow(),
        }, cancellationToken);
    }
}
