using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;

using RestSharp;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class PlayerEventsApi : BaseApi<EventIngestApiClientOptions>, IPlayerEventsApi
{
    public PlayerEventsApi(
        ILogger<BaseApi<EventIngestApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        EventIngestApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult> OnPlayerConnected(OnPlayerConnected onPlayerConnected, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/OnPlayerConnected", Method.Post, cancellationToken);
            request.AddJsonBody(onPlayerConnected);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to send player connected event"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult> OnChatMessage(OnChatMessage onChatMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/OnChatMessage", Method.Post, cancellationToken);
            request.AddJsonBody(onChatMessage);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to send chat message event"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult> OnMapVote(OnMapVote onMapVote, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/OnMapVote", Method.Post, cancellationToken);
            request.AddJsonBody(onMapVote);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to send map vote event"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
