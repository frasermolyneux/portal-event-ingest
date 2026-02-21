using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

public interface IServerEventsApi
{
    Task<ApiResult> OnServerConnected(OnServerConnected onServerConnected, CancellationToken cancellationToken = default);
    Task<ApiResult> OnMapChange(OnMapChange onMapChange, CancellationToken cancellationToken = default);
}
