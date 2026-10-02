# Wires and Pipes

A live infographic of physical energy flows into and out of Great Britain: electricity (and later, gas and LNG). See the GitHub issues for the spec (#1) and the vertical-slice tickets.

## Running locally

Requires the .NET SDK, Postgres, and RabbitMQ reachable at the connection strings in `src/WiresAndPipes.Api/appsettings.json` (or override via environment/user secrets).

```
dotnet run --project src/WiresAndPipes.Api
```

The frontend is plain TypeScript, compiled ahead of time into `src/WiresAndPipes.Api/wwwroot/js/` and served as static files by the same process — no separate frontend build or host. To rebuild it after changing `frontend/src/app.ts`:

```
cd frontend
npm install
npm run build
```

The compiled output is committed for now, since there's no deploy pipeline yet to generate it; a later ticket (deployment) should move the `npm run build` step into the Docker image build instead.

## Testing

```
dotnet test
```

The integration tests use [Testcontainers](https://testcontainers.com/) to spin up real Postgres and RabbitMQ instances, so a Docker daemon must be reachable from wherever `dotnet test` runs.

## Why MassTransit v8

The RabbitMQ messaging layer uses **MassTransit v8**, pinned, which is Apache-2.0 licensed. MassTransit v9 moved to a commercial license (a free tier exists for organisations under roughly $1M revenue, but it requires a license key at runtime). Usage here is deliberately thin — one publish call from the poller, one consumer — so that switching to an alternative like Wolverine or Rebus later, if v8 support lapses, stays cheap.

MassTransit v8.5.11 builds and runs cleanly against .NET 10 (confirmed by this slice's own build and integration test run); no compatibility issue was found.
