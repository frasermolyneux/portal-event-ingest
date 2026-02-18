namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;

internal record PlayerContext(Guid PlayerId, DateTime FirstSeen, bool HasModerateChatTag);
