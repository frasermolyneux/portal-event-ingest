using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public class FakeServerEventsApi : IServerEventsApi
{
    public ConcurrentBag<OnServerConnected> ServerConnectedEvents { get; } = new();
    public ConcurrentBag<OnMapChange> MapChangeEvents { get; } = new();

    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

    public FakeServerEventsApi WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public void Reset()
    {
        ServerConnectedEvents.Clear();
        MapChangeEvents.Clear();
        StatusCode = HttpStatusCode.OK;
    }

    public Task<ApiResult> OnServerConnected(OnServerConnected onServerConnected, CancellationToken cancellationToken = default)
    {
        ServerConnectedEvents.Add(onServerConnected);
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }

    public Task<ApiResult> OnMapChange(OnMapChange onMapChange, CancellationToken cancellationToken = default)
    {
        MapChangeEvents.Add(onMapChange);
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }
}
