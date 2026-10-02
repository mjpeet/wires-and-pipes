namespace WiresAndPipes.Api.Polling;

/// <summary>
/// One fetch-and-publish cycle, factored out of the timer loop so tests can trigger a single
/// cycle deterministically instead of waiting on the schedule.
/// </summary>
public interface IElexonPollCycle
{
    Task RunOnceAsync(CancellationToken cancellationToken);
}
