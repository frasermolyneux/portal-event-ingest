﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace XtremeIdiots.Portal.Events.Ingest.App.Functions.V1
{
    public class HealthCheck
    {
        private readonly HealthCheckService healthCheck;

        public HealthCheck(HealthCheckService healthCheck)
        {
            this.healthCheck = healthCheck;
        }

        [Function(nameof(HealthCheck))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/health")] HttpRequestData req,
            FunctionContext context)
        {
            var healthStatus = await healthCheck.CheckHealthAsync();
            return new OkObjectResult(Enum.GetName(typeof(HealthStatus), healthStatus.Status));
        }
    }
}
