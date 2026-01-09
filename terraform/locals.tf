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

  app_configuration_endpoint = data.terraform_remote_state.portal_environments.outputs.app_configuration.endpoint

  managed_identities            = data.terraform_remote_state.portal_environments.outputs.managed_identities
  event_ingest_funcapp_identity = local.managed_identities["event_ingest_funcapp_identity"]
  api_management_identity       = local.managed_identities["environments_api_management_identity"]

  api_management   = data.terraform_remote_state.portal_environments.outputs.api_management
  event_ingest_api = data.terraform_remote_state.portal_environments.outputs.event_ingest_api
  repository_api   = data.terraform_remote_state.portal_environments.outputs.repository_api
  app_insights     = data.terraform_remote_state.portal_core.outputs.app_insights
  app_service_plan = data.terraform_remote_state.portal_core.outputs.app_service_plans["apps"]

  # Local Resource Naming
  function_app_name         = "fn-portal-event-ingest-${var.environment}-${var.location}-${random_id.environment_id.hex}"
  function_app_storage_name = "safn${random_id.environment_id.hex}"
  service_bus_name          = substr(format("sb-portal-event-ingest-%s-%s-%s-%s", var.environment, var.location, random_id.environment_id.hex, random_id.environment_id.hex), 0, 50)
  dashboard_name            = "portal-event-ingest-${var.environment}-${random_id.environment_id.hex}"
}
