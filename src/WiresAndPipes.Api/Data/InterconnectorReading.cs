namespace WiresAndPipes.Api.Data;

/// <summary>
/// Normalised read model for a single interconnector's settlement-period reading. One row per
/// (interconnector, settlement date, settlement period); the consumer upserts on that key as
/// fresher Elexon data arrives.
/// </summary>
public sealed class InterconnectorReading
{
    public int Id { get; set; }
    public required string InterconnectorCode { get; set; }
    public required DateOnly SettlementDate { get; set; }
    public required int SettlementPeriod { get; set; }
    public required decimal GenerationMw { get; set; }
    public required DateTimeOffset RecordedAt { get; set; }
}
