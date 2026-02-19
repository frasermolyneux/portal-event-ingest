using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ApiInfo
{
    [Function(nameof(ApiInfo))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/info")] HttpRequestData req,
        FunctionContext context)
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

        return new OkObjectResult(new { buildVersion = version });
    }
}
