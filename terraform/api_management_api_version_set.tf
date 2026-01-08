resource "azurerm_api_management_api_version_set" "event_ingest_api_version_set" {
  name = local.event_ingest_api.root_path

  resource_group_name = data.azurerm_api_management.api_management.resource_group_name
  api_management_name = data.azurerm_api_management.api_management.name

  display_name      = "Event Ingest API"
  versioning_scheme = "Segment"
}
