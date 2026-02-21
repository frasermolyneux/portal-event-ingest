using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Models;

namespace XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

public interface IApiInfoApi
{
    Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default);
}
