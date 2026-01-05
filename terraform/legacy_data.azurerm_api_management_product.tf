data "azurerm_api_management_product" "repository_api_product" {
  product_id = var.repository_api.apim_product_id

  api_management_name = local.core_api_management.name
  resource_group_name = local.core_api_management.resource_group_name
}
