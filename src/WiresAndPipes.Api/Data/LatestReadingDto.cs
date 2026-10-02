namespace WiresAndPipes.Api.Data;

public sealed record LatestReadingDto(
    string InterconnectorCode,
    DateOnly SettlementDate,
    int SettlementPeriod,
    decimal GenerationMw,
    DateTimeOffset RecordedAt);
