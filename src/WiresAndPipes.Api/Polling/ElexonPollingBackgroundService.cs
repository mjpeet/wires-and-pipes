using Microsoft.Extensions.Options;

namespace WiresAndPipes.Api.Polling;

public sealed class ElexonPollingBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<ElexonPollingOptions> options,
    ILogger<ElexonPollingBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds));

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var pollCycle = scope.ServiceProvider.GetRequiredService<IElexonPollCycle>();
                await pollCycle.RunOnceAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Elexon poll cycle failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
