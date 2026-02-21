using Microsoft.Extensions.Logging;

using MX.Api.Abstractions;
using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Extensions;

using RestSharp;

using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.V1;

public class ApiInfoApi : BaseApi<EventIngestApiClientOptions>, IApiInfoApi
{
    public ApiInfoApi(
        ILogger<BaseApi<EventIngestApiClientOptions>> logger,
        IApiTokenProvider? apiTokenProvider,
        IRestClientService restClientService,
        EventIngestApiClientOptions options)
        : base(logger, apiTokenProvider, restClientService, options)
    {
    }

    public async Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await CreateRequestAsync("v1/info", Method.Get, cancellationToken);
            var response = await ExecuteAsync(request, cancellationToken);

            return response.ToApiResult<ApiInfoDto>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var errorResponse = new ApiResponse<ApiInfoDto>(
                new ApiError("CLIENT_ERROR", "Failed to retrieve API info"));
            return new ApiResult<ApiInfoDto>(System.Net.HttpStatusCode.InternalServerError, errorResponse);
        }
    }
}
