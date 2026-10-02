using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using WiresAndPipes.Api.Elexon;
using WiresAndPipes.Api.Polling;
using Xunit;

namespace WiresAndPipes.IntegrationTests;

/// <summary>
/// Boots the real API against real, disposable Postgres and RabbitMQ containers, with the
/// Elexon HTTP call swapped for a stub. This is the seam the spec calls for: everything from
/// the poller onward is real, only the external Elexon API is faked.
/// </summary>
public sealed class ElectricityApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3-management-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var rabbitMqUri = new Uri(_rabbitMq.GetConnectionString());
            var userInfo = rabbitMqUri.UserInfo.Split(':', 2);

            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["RabbitMq:Host"] = rabbitMqUri.Host,
                ["RabbitMq:Port"] = rabbitMqUri.Port.ToString(),
                ["RabbitMq:Username"] = Uri.UnescapeDataString(userInfo[0]),
                ["RabbitMq:Password"] = Uri.UnescapeDataString(userInfo[1]),
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IElexonClient));
            services.AddSingleton<IElexonClient, StubElexonClient>();

            // The background poller runs on its own schedule; tests trigger a poll cycle
            // explicitly instead, so remove just that hosted service (not MassTransit's own,
            // which must keep running for publish/consume to work).
            var pollingHostedService = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IHostedService)
                && d.ImplementationType == typeof(ElexonPollingBackgroundService));
            if (pollingHostedService is not null)
            {
                services.Remove(pollingHostedService);
            }
        });
    }
}
