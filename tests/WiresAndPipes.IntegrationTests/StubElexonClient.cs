using WiresAndPipes.Api.Elexon;

namespace WiresAndPipes.IntegrationTests;

/// <summary>
/// Stands in for the real Elexon API in tests: returns a fixed, recorded-looking FUELHH
/// payload instead of making a network call.
/// </summary>
public sealed class StubElexonClient : IElexonClient
{
    public const string TrackedInterconnectorCode = "INTFR";
    public const decimal TrackedInterconnectorGenerationMw = 742.5m;
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
              "fuelType": "{{TrackedInterconnectorCode}}",
              "generation": {{TrackedInterconnectorGenerationMw}}
            },
            {
              "settlementDate": "{{SettlementDate}}",
              "settlementPeriod": {{SettlementPeriod}},
              "fuelType": "INTNED",
              "generation": 310.0
            }
          ]
        }
        """;

        return Task.FromResult(json);
    }
}
