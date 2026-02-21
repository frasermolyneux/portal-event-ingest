using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;

using RestSharp;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class ApiHealthApi : BaseApi<EventIngestApiClientOptions>, IApiHealthApi
{
    public ApiHealthApi(
        ILogger<BaseApi<EventIngestApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        EventIngestApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/health", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse(
                new ApiError("CLIENT_ERROR", "Failed to check API health"));
            return new ApiResult(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
