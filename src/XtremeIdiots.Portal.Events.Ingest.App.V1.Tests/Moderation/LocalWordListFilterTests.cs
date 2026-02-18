using XtremeIdiots.Portal.Events.Ingest.App.V1.Moderation;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.Moderation;

public class LocalWordListFilterTests
{
    private readonly LocalWordListFilter _sut = new();

    [Fact]
    public void Check_WithCleanMessage_ReturnsNull()
    {
        // Act
        var result = _sut.Check("hello how are you today");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Check_WithEmptyMessage_ReturnsNull()
    {
        // Act
        var result = _sut.Check("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Check_WithPlaceholderTerm_ReturnsMatch()
    {
        // Act
        var result = _sut.Check("this is example-placeholder-term in a message");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Placeholder", result.MatchedCategory);
        Assert.Equal("example-placeholder-term", result.MatchedTerm);
    }

    [Fact]
    public void Check_WithPlaceholderTermAtStart_ReturnsMatch()
    {
        // Act
        var result = _sut.Check("example-placeholder-term is bad");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Placeholder", result.MatchedCategory);
    }

    [Fact]
    public void Check_WithPlaceholderTermAtEnd_ReturnsMatch()
    {
        // Act
        var result = _sut.Check("someone said example-placeholder-term");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Check_IsCaseInsensitive()
    {
        // Act
        var result = _sut.Check("EXAMPLE-PLACEHOLDER-TERM");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Placeholder", result.MatchedCategory);
    }

    [Fact]
    public void Check_WithTermAsSubstring_DoesNotMatch()
    {
        // The word boundary check should prevent matching substrings
        // "example-placeholder-termination" should not match "example-placeholder-term"
        // because the character after the term is a letter
        var result = _sut.Check("example-placeholder-termination");

        Assert.Null(result);
    }

    [Fact]
    public void Check_WithTermFollowedByPunctuation_ReturnsMatch()
    {
        // Act
        var result = _sut.Check("he said example-placeholder-term!");

        // Assert
        Assert.NotNull(result);
    }
}
