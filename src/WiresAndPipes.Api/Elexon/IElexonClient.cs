namespace WiresAndPipes.Api.Elexon;

/// <summary>
/// Fetches the raw FUELHH dataset body from Elexon. Returns the untouched JSON text so the
/// poller can publish it as-is; parsing happens downstream in the consumer.
/// </summary>
public interface IElexonClient
{
    Task<string> GetFuelHhRawAsync(CancellationToken cancellationToken);
}
