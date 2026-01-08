data "azurerm_api_management_product" "repository_api_product" {
  product_id = local.repository_api.api_management.root_path

  api_management_name = local.api_management.name
  resource_group_name = local.api_management.resource_group_name
}
