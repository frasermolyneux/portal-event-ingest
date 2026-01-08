data "azurerm_api_management" "api_management" {
  name                = locals.api_management.name
  resource_group_name = locals.api_management.resource_group_name
}
