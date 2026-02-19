using System.Net;
using System.Reflection;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using XtremeIdiots.Portal.Events.Ingest.App.Models;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class ApiInfo
{
    [Function(nameof(ApiInfo))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/info")] HttpRequestData req,
        FunctionContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new ApiInfoDto
        {
            Version = informationalVersion,
            BuildVersion = informationalVersion.Split('+')[0],
            AssemblyVersion = assemblyVersion
        });
        return response;
    }
}
