locals {
  # Remote State References
  workload_resource_groups = {
    for location in [var.location] :
    location => data.terraform_remote_state.platform_workloads.outputs.workload_resource_groups[var.workload_name][var.environment].resource_groups[lower(location)]
  }

  workload_backend = try(
    data.terraform_remote_state.platform_workloads.outputs.workload_terraform_backends[var.workload_name][var.environment],
    null
  )

  workload_administrative_unit = try(
    data.terraform_remote_state.platform_workloads.outputs.workload_administrative_units[var.workload_name][var.environment],
    null
  )

  workload_resource_group = local.workload_resource_groups[var.location]

  # Local Resource Naming
  resource_group_name       = "rg-portal-event-ingest-${var.environment}-${var.location}-${var.instance}"
  key_vault_name            = substr(format("kv-%s-%s", random_id.environment_id.hex, var.location), 0, 24)
  function_app_name         = "fn-portal-event-ingest-${var.environment}-${var.location}-${var.instance}-${random_id.environment_id.hex}"
  function_app_storage_name = "safn${random_id.environment_id.hex}"
  app_service_plan_name     = "asp-portal-event-ingest-${var.environment}-${var.location}-${var.instance}"
  service_bus_name          = substr(format("sb-portal-event-ingest-%s-%s-%s-%s", var.environment, var.location, var.instance, random_id.environment_id.hex), 0, 50)
  app_registration_name     = "portal-event-ingest-${var.environment}-${var.instance}"
  dashboard_name            = "portal-event-ingest-${var.environment}-${var.instance}"
}
