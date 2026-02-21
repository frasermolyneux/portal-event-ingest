using XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing;

namespace XtremeIdiots.Portal.Events.Ingest.Api.Client.Testing.Tests;

[Trait("Category", "Unit")]
public class EventIngestDtoFactoryTests
{
    [Fact]
    public void CreateApiInfo_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateApiInfo();

        Assert.NotNull(dto.Version);
        Assert.NotNull(dto.BuildVersion);
        Assert.NotNull(dto.AssemblyVersion);
    }

    [Fact]
    public void CreateApiInfo_WithCustomValues_ReturnsCustomDto()
    {
        var dto = EventIngestDtoFactory.CreateApiInfo(buildVersion: "5.0.0");

        Assert.Equal("5.0.0", dto.BuildVersion);
    }

    [Fact]
    public void CreateOnPlayerConnected_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateOnPlayerConnected();

        Assert.Equal("TestPlayer", dto.Username);
        Assert.NotNull(dto.Guid);
        Assert.NotNull(dto.IpAddress);
        Assert.NotNull(dto.GameType);
    }

    [Fact]
    public void CreateOnPlayerConnected_WithCustomValues_ReturnsCustomDto()
    {
        var dto = EventIngestDtoFactory.CreateOnPlayerConnected(username: "CustomPlayer", gameType: "CallOfDuty2");

        Assert.Equal("CustomPlayer", dto.Username);
        Assert.Equal("CallOfDuty2", dto.GameType);
    }

    [Fact]
    public void CreateOnChatMessage_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateOnChatMessage();

        Assert.Equal("TestPlayer", dto.Username);
        Assert.Equal("Hello world", dto.Message);
        Assert.Equal("say", dto.Type);
    }

    [Fact]
    public void CreateOnMapVote_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateOnMapVote();

        Assert.Equal("mp_crossfire", dto.MapName);
        Assert.True(dto.Like);
    }

    [Fact]
    public void CreateOnServerConnected_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateOnServerConnected();

        Assert.NotNull(dto.Id);
        Assert.Equal("CallOfDuty4", dto.GameType);
    }

    [Fact]
    public void CreateOnMapChange_WithDefaults_ReturnsValidDto()
    {
        var dto = EventIngestDtoFactory.CreateOnMapChange();

        Assert.Equal("Call of Duty 4", dto.GameName);
        Assert.Equal("mp_crossfire", dto.MapName);
    }
}
