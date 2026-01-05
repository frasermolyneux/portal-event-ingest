resource "azurerm_api_management_subscription" "legacy_repository_api_subscription" {
  api_management_name = local.core_api_management.name
  resource_group_name = local.core_api_management.resource_group_name

  state         = "active"
  allow_tracing = false

  product_id   = format("%s/products/%s", local.core_api_management.id, local.event_ingest_api_shared.product_id)
  display_name = format("%s-%s", local.legacy_function_app_name, local.event_ingest_api_shared.product_id)
}
