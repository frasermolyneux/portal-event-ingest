using System.Reflection;
using System.Text.Json;

using XtremeIdiots.Portal.Events.Ingest.App.V1.OpenApi;

namespace XtremeIdiots.Portal.Events.Ingest.App.V1.Tests.OpenApi;

public class OpenApiDocumentGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_WithAppAssembly_ShouldProducePathsAndValidJson()
    {
        var appAssembly = typeof(OpenApiDocumentGenerator).Assembly;
        var result = await OpenApiDocumentGenerator.GenerateAsync(appAssembly);
        Assert.NotNull(result);

        var doc = JsonDocument.Parse(result);
        var paths = doc.RootElement.GetProperty("paths");
        Assert.NotEqual("{}", paths.GetRawText());
    }
}

