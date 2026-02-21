using System.Net;
using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;
using XtremeIdiots.Portal.Events.Abstractions.Models;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

public class FakeApiHealthApi : IApiHealthApi
{
    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.OK;

    public FakeApiHealthApi WithStatusCode(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public void Reset()
    {
        StatusCode = HttpStatusCode.OK;
    }

    public Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ApiResult(StatusCode, new ApiResponse()));
    }
}

public class FakeApiInfoApi : IApiInfoApi
{
    private ApiInfoDto _info = EventIngestDtoFactory.CreateApiInfo();

    public FakeApiInfoApi WithInfo(ApiInfoDto info)
    {
        _info = info;
        return this;
    }

    public void Reset()
    {
        _info = EventIngestDtoFactory.CreateApiInfo();
    }

    public Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ApiResult<ApiInfoDto>(HttpStatusCode.OK, new ApiResponse<ApiInfoDto>(_info)));
    }
}
