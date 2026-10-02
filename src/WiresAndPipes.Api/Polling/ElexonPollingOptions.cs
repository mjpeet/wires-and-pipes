namespace WiresAndPipes.Api.Polling;

public sealed class ElexonPollingOptions
{
    public const string SectionName = "Elexon";

    public int PollIntervalSeconds { get; init; } = 300;
}
