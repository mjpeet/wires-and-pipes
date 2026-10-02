namespace WiresAndPipes.Api.Polling;

public sealed class ElexonPollingOptions
{
    public const string SectionName = "Elexon";

    public required string TrackedInterconnectorCode { get; init; }
    public int PollIntervalSeconds { get; init; } = 300;
}
