using MX.Api.Abstractions;

namespace XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

public interface IApiHealthApi
{
    Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default);
}
