using MX.Api.Abstractions;
using XtremeIdiots.Portal.Events.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Events.Abstractions.Interfaces.V1;

public interface IPlayerEventsApi
{
    Task<ApiResult> OnPlayerConnected(OnPlayerConnected onPlayerConnected, CancellationToken cancellationToken = default);
    Task<ApiResult> OnChatMessage(OnChatMessage onChatMessage, CancellationToken cancellationToken = default);
    Task<ApiResult> OnMapVote(OnMapVote onMapVote, CancellationToken cancellationToken = default);
}
