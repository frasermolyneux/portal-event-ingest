output "function_app_name" {
  value = azurerm_linux_function_app.legacy_app.name
}

output "resource_group_name" {
  value = azurerm_resource_group.legacy_rg.name
}

output "staging_dashboard_name" {
  value = var.environment == "dev" ? azurerm_portal_dashboard.legacy_staging_dashboard[0].name : ""
}

output "event_ingest_api" {
  value = {
    version_set_id      = azurerm_api_management_api_version_set.legacy_event_ingest_api_version_set.id
    product_id          = azurerm_api_management_product.legacy_event_ingest_api_product.product_id
    product_resource_id = "${local.core_api_management.id}/products/${azurerm_api_management_product.legacy_event_ingest_api_product.product_id}"
  }
}
