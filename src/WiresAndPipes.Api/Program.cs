using MassTransit;
using Microsoft.EntityFrameworkCore;
using WiresAndPipes.Api.Consumers;
using WiresAndPipes.Api.Data;
using WiresAndPipes.Api.Elexon;
using WiresAndPipes.Api.Polling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<ElexonPollingOptions>(
    builder.Configuration.GetSection(ElexonPollingOptions.SectionName));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElexonPollingOptions>>().Value);

builder.Services.AddHttpClient<IElexonClient, ElexonClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Elexon:BaseUrl"]
        ?? "https://data.elexon.co.uk/bmrs/api/v1/");
});

builder.Services.AddDbContext<WiresAndPipesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IElexonPollCycle, ElexonPollCycle>();
builder.Services.AddHostedService<ElexonPollingBackgroundService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FuelHhRawDataReceivedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqPort = builder.Configuration.GetValue<ushort?>("RabbitMq:Port") ?? 5672;

        cfg.Host(
            builder.Configuration["RabbitMq:Host"] ?? "localhost",
            rabbitMqPort,
            builder.Configuration["RabbitMq:VirtualHost"] ?? "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WiresAndPipesDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/interconnectors/{code}/latest", async (string code, WiresAndPipesDbContext dbContext) =>
{
    var latest = await dbContext.InterconnectorReadings
        .Where(r => r.InterconnectorCode == code)
        .OrderByDescending(r => r.SettlementDate)
        .ThenByDescending(r => r.SettlementPeriod)
        .Select(r => new
        {
            r.InterconnectorCode,
            r.SettlementDate,
            r.SettlementPeriod,
            r.GenerationMw,
            r.RecordedAt,
        })
        .FirstOrDefaultAsync();

    return latest is null ? Results.NotFound() : Results.Ok(latest);
});

app.Run();

public partial class Program;
