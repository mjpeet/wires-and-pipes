namespace WiresAndPipes.Api.Elexon;

/// <summary>
/// Shape of one record in the Elexon FUELHH dataset response, and of the response envelope.
/// Field names follow the Elexon Insights API's half-hourly-by-fuel-type convention
/// (dataset/settlementDate/settlementPeriod/fuelType/generation). Not yet verified against a
/// live payload — see the "Open facts" in the project spec. Confirm before relying on this in
/// production and adjust field names if the live API disagrees.
/// </summary>
public sealed record FuelHhRecord
{
    public required string SettlementDate { get; init; }
    public required int SettlementPeriod { get; init; }
    public required string FuelType { get; init; }
    public required decimal Generation { get; init; }
}

public sealed record FuelHhResponse
{
    public required IReadOnlyList<FuelHhRecord> Data { get; init; }
}
