locals {
  resource_group_name       = "rg-portal-event-ingest-${var.environment}-${var.location}"
  key_vault_name            = "kv-${random_id.environment_id.hex}-${var.location}"
  app_insights_name         = "ai-ptl-event-ingest-${random_id.environment_id.hex}-${var.environment}-${var.location}"
  function_app_name         = "fa-ptl-event-ingest-${random_id.environment_id.hex}-${var.environment}-${var.location}"
  function_app_storage_name = "saptlevntinfn${random_id.environment_id.hex}"
  service_bus_name          = "sb-ptl-event-ingest-${random_id.environment_id.hex}-${var.environment}-${var.location}"
}
