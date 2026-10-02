using WiresAndPipes.Api.Elexon;

namespace WiresAndPipes.IntegrationTests;

/// <summary>
/// Stands in for the real Elexon API in tests: returns a fixed, recorded-looking FUELHH
/// payload instead of making a network call. Includes several interconnectors plus one
/// domestic fuel type, so tests can assert that only interconnectors get normalised.
/// </summary>
public sealed class StubElexonClient : IElexonClient
{
    public const string PrimaryInterconnectorCode = "INTFR";
    public const decimal PrimaryInterconnectorGenerationMw = 742.5m;
    public const string SecondaryInterconnectorCode = "INTNED";
    public const decimal SecondaryInterconnectorGenerationMw = -310.0m;
    public const string DomesticFuelType = "WIND";
    public const string SettlementDate = "2026-10-02";
    public const int SettlementPeriod = 17;

    public Task<string> GetFuelHhRawAsync(CancellationToken cancellationToken)
    {
        var json = $$"""
        {
          "data": [
            {
              "settlementDate": "{{SettlementDate}}",
              "settlementPeriod": {{SettlementPeriod}},
              "fuelType": "{{PrimaryInterconnectorCode}}",
              "generation": {{PrimaryInterconnectorGenerationMw}}
            },
            {
              "settlementDate": "{{SettlementDate}}",
              "settlementPeriod": {{SettlementPeriod}},
              "fuelType": "{{SecondaryInterconnectorCode}}",
              "generation": {{SecondaryInterconnectorGenerationMw}}
            },
            {
              "settlementDate": "{{SettlementDate}}",
              "settlementPeriod": {{SettlementPeriod}},
              "fuelType": "{{DomesticFuelType}}",
              "generation": 5102.0
            }
          ]
        }
        """;

        return Task.FromResult(json);
    }
}
