using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using XtremeIdiots.Portal.Events.Ingest.App.V1.OpenApi;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class OpenApi
{
    [Function(nameof(OpenApi))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi/{filename}")] HttpRequestData req,
        FunctionContext context, string filename)
    {
        if (!string.Equals(filename, "v1.json", StringComparison.OrdinalIgnoreCase))
        {
            return new NotFoundResult();
        }

        var content = await OpenApiDocumentGenerator.GenerateAsync();

        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}
