using System.Net.Http.Json;

namespace WiresAndPipes.IntegrationTests;

/// <summary>
/// Consumption through RabbitMQ is asynchronous, so tests poll the HTTP API rather than
/// asserting immediately after triggering a poll cycle.
/// </summary>
public static class HttpPolling
{
    public static async Task<T> PollUntilAsync<T>(
        HttpClient client, string requestUri, Func<T, bool> isReady, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                if (result is not null && isReady(result))
                {
                    return result;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250));
        }

        throw new TimeoutException($"Condition not met for {requestUri} within {timeout}.");
    }
}
