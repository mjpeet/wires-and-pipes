namespace WiresAndPipes.IntegrationTests;

/// <summary>Shape returned by both the single- and all-interconnector "latest" endpoints.</summary>
public sealed record LatestReadingResponse(
    string InterconnectorCode,
    DateOnly SettlementDate,
    int SettlementPeriod,
    decimal GenerationMw,
    DateTimeOffset RecordedAt);
