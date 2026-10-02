namespace WiresAndPipes.Api.Elexon;

public sealed class ElexonClient(HttpClient httpClient, TimeProvider timeProvider) : IElexonClient
{
    public async Task<string> GetFuelHhRawAsync(CancellationToken cancellationToken)
    {
        var today = timeProvider.GetUtcNow().Date;
        var dateParam = today.ToString("yyyy-MM-dd");

        using var response = await httpClient.GetAsync(
            $"datasets/FUELHH?settlementDateFrom={dateParam}&settlementDateTo={dateParam}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
