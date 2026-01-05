data "azurerm_resource_group" "core" {
  name = local.core_api_management.resource_group_name
}
