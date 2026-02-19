using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1;

public class HealthCheck(HealthCheckService healthCheck)
{
    [Function(nameof(HealthCheck))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req,
        FunctionContext context)
    {
        var healthStatus = await healthCheck.CheckHealthAsync().ConfigureAwait(false);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(healthStatus.Status.ToString());
        return response;
    }
}
