using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using WiresAndPipes.Api.Data;
using WiresAndPipes.Api.Elexon;

namespace WiresAndPipes.Api.Consumers;

/// <summary>
/// Normalises a raw FUELHH payload into the Postgres read model, keeping every interconnector
/// (see <see cref="InterconnectorCodes"/>) and discarding domestic fuel types. Upserts on
/// (interconnector, settlement date, settlement period) so re-polling the same period is
/// idempotent.
/// </summary>
public sealed class FuelHhRawDataReceivedConsumer(WiresAndPipesDbContext dbContext)
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

        var interconnectorRecords = payload.Data.Where(r => InterconnectorCodes.IsInterconnector(r.FuelType));

        foreach (var record in interconnectorRecords)
        {
            var settlementDate = DateOnly.Parse(record.SettlementDate);

            var existing = await dbContext.InterconnectorReadings.SingleOrDefaultAsync(
                r => r.InterconnectorCode == record.FuelType
                     && r.SettlementDate == settlementDate
                     && r.SettlementPeriod == record.SettlementPeriod,
                context.CancellationToken);

            if (existing is null)
            {
                dbContext.InterconnectorReadings.Add(new InterconnectorReading
                {
                    InterconnectorCode = record.FuelType,
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
