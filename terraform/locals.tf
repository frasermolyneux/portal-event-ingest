locals {
  resource_group_name       = "rg-portal-event-ingest-${var.environment}-${var.location}-${var.instance}"
  key_vault_name            = "kv-${random_id.environment_id.hex}-${var.location}"
  app_insights_name         = "ai-portal-event-ingest-${var.environment}-${var.location}-${var.instance}"
  workload_name             = "app-portal-event-ingest-${var.environment}-${var.instance}-${random_id.environment_id.hex}"
  function_app_name         = "fn-portal-event-ingest-${var.environment}-${var.location}-${var.instance}-${random_id.environment_id.hex}"
  function_app_storage_name = "safn${random_id.environment_id.hex}"
  service_bus_name          = "sb-portal-event-ingest-${var.environment}-${var.location}-${var.instance}-${random_id.environment_id.hex}"
  app_registration_name     = "portal-event-ingest-${var.environment}"
}
