namespace WiresAndPipes.Api.Elexon;

/// <summary>
/// Elexon has no dedicated "is this an interconnector" field; every interconnector fuel type
/// observed so far shares the "INT" prefix, which domestic fuel types (WIND, CCGT, ...) never
/// do. Filtering on the prefix means a newly commissioned interconnector shows up
/// automatically, with no code change needed.
///
/// Confirmed against a live FUELHH payload during development (2026-10-02): the full set
/// observed was INTELEC, INTEW, INTFR, INTGRNL, INTIFA2, INTIRL, INTNED, INTNEM, INTNSL,
/// INTVKL — the same 10 codes this project's spec assumed. No other "INT*" code was seen.
/// </summary>
public static class InterconnectorCodes
{
    private const string Prefix = "INT";

    public static bool IsInterconnector(string fuelType) =>
        fuelType.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
}
