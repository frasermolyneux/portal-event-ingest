using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public class FakePlayerEventsApi : IPlayerEventsApi
{
    public ConcurrentBag<OnPlayerConnected> PlayerConnectedEvents { get; } = new();
    public ConcurrentBag<OnChatMessage> ChatMessageEvents { get; } = new();
    public ConcurrentBag<OnMapVote> MapVoteEvents { get; } = new();

    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

    public FakePlayerEventsApi WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public void Reset()
    {
        PlayerConnectedEvents.Clear();
        ChatMessageEvents.Clear();
        MapVoteEvents.Clear();
        StatusCode = HttpStatusCode.OK;
    }

    public Task<ApiResult> OnPlayerConnected(OnPlayerConnected onPlayerConnected, CancellationToken cancellationToken = default)
    {
        PlayerConnectedEvents.Add(onPlayerConnected);
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }

    public Task<ApiResult> OnChatMessage(OnChatMessage onChatMessage, CancellationToken cancellationToken = default)
    {
        ChatMessageEvents.Add(onChatMessage);
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }

    public Task<ApiResult> OnMapVote(OnMapVote onMapVote, CancellationToken cancellationToken = default)
    {
        MapVoteEvents.Add(onMapVote);
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }
}
