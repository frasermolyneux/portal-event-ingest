resource "azurerm_api_management_subscription" "legacy_repository_api_subscription" {
  api_management_name = local.core_api_management.name
  resource_group_name = local.core_api_management.resource_group_name

  state         = "active"
  allow_tracing = false

  product_id   = data.azurerm_api_management_product.repository_api_product.id
  display_name = format("%s-%s", local.legacy_function_app_name, var.repository_api.apim_product_id)
}
