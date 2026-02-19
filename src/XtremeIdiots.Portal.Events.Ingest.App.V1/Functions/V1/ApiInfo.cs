using System.Net;
using System.Reflection;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ApiInfo
{
    [Function(nameof(ApiInfo))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/info")] HttpRequestData req,
        FunctionContext context)
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";

        // Strip git commit metadata suffix (e.g., "1.1.1+abc1234" → "1.1.1") to match NuGetPackageVersion
        var plusIndex = version.IndexOf('+');
        if (plusIndex >= 0) version = version[..plusIndex];

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { buildVersion = version });
        return response;
    }
}
