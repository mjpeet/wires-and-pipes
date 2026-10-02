using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using WiresAndPipes.Api.Data;
using WiresAndPipes.Api.Elexon;
using WiresAndPipes.Api.Polling;

namespace WiresAndPipes.Api.Consumers;

/// <summary>
/// Normalises a raw FUELHH payload into the Postgres read model, keeping only the tracked
/// interconnector for this slice. Upserts on (interconnector, settlement date, settlement
/// period) so re-polling the same period is idempotent.
/// </summary>
public sealed class FuelHhRawDataReceivedConsumer(
    WiresAndPipesDbContext dbContext,
    ElexonPollingOptions options)
    : IConsumer<FuelHhRawDataReceived>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Consume(ConsumeContext<FuelHhRawDataReceived> context)
    {
        var payload = JsonSerializer.Deserialize<FuelHhResponse>(context.Message.RawJson, JsonOptions);
        if (payload is null)
        {
            return;
        }

        var trackedRecords = payload.Data.Where(r =>
            string.Equals(r.FuelType, options.TrackedInterconnectorCode, StringComparison.OrdinalIgnoreCase));

        foreach (var record in trackedRecords)
        {
            var settlementDate = DateOnly.Parse(record.SettlementDate);

            var existing = await dbContext.InterconnectorReadings.SingleOrDefaultAsync(
                r => r.InterconnectorCode == options.TrackedInterconnectorCode
                     && r.SettlementDate == settlementDate
                     && r.SettlementPeriod == record.SettlementPeriod,
                context.CancellationToken);

            if (existing is null)
            {
                dbContext.InterconnectorReadings.Add(new InterconnectorReading
                {
                    InterconnectorCode = options.TrackedInterconnectorCode,
                    SettlementDate = settlementDate,
                    SettlementPeriod = record.SettlementPeriod,
                    GenerationMw = record.Generation,
                    RecordedAt = context.Message.RetrievedAt,
                });
            }
            else
            {
                existing.GenerationMw = record.Generation;
                existing.RecordedAt = context.Message.RetrievedAt;
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
