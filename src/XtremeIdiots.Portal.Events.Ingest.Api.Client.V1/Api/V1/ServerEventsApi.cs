using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;

using RestSharp;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class ServerEventsApi : BaseApi<EventIngestApiClientOptions>, IServerEventsApi
{
    public ServerEventsApi(
        ILogger<BaseApi<EventIngestApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        EventIngestApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult> OnServerConnected(OnServerConnected onServerConnected, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/OnServerConnected", Method.Post, cancellationToken);
            request.AddJsonBody(onServerConnected);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to send server connected event"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }

    public async Task<ApiResult> OnMapChange(OnMapChange onMapChange, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/OnMapChange", Method.Post, cancellationToken);
            request.AddJsonBody(onMapChange);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to send map change event"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
