using Microsoft.EntityFrameworkCore;

namespace WiresAndPipes.Api.Data;

/// <summary>
/// The "latest reading" shape the API exposes, shared by the single-interconnector and
/// all-interconnectors endpoints so they can't drift apart.
/// </summary>
public static class InterconnectorReadingQueries
{
    public static Task<LatestReadingDto?> GetLatestForInterconnectorAsync(
        this WiresAndPipesDbContext dbContext, string interconnectorCode, CancellationToken cancellationToken) =>
        dbContext.InterconnectorReadings
            .Where(r => r.InterconnectorCode == interconnectorCode)
            .OrderByDescending(r => r.SettlementDate)
            .ThenByDescending(r => r.SettlementPeriod)
            .Select(r => new LatestReadingDto(r.InterconnectorCode, r.SettlementDate, r.SettlementPeriod, r.GenerationMw, r.RecordedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public static async Task<List<LatestReadingDto>> GetLatestForAllInterconnectorsAsync(
        this WiresAndPipesDbContext dbContext, CancellationToken cancellationToken)
    {
        var interconnectorCodes = await dbContext.InterconnectorReadings
            .Select(r => r.InterconnectorCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        var latestReadings = new List<LatestReadingDto>();
        foreach (var code in interconnectorCodes)
        {
            var latest = await dbContext.GetLatestForInterconnectorAsync(code, cancellationToken);
            if (latest is not null)
            {
                latestReadings.Add(latest);
            }
        }

        return latestReadings;
    }
}
