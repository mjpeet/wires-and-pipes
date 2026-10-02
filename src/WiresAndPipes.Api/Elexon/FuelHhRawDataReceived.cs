namespace WiresAndPipes.Api.Elexon;

/// <summary>
/// Immutable event carrying exactly what Elexon returned, unmodified. Published by the poller,
/// consumed by the normaliser. Keeping the raw body lets the read model be rebuilt from scratch
/// if normalisation logic changes later, without re-polling Elexon.
/// </summary>
public sealed record FuelHhRawDataReceived
{
    public required string RawJson { get; init; }
    public required DateTimeOffset RetrievedAt { get; init; }
}
