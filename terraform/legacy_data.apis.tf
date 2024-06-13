moved {
  from = azurerm_api_management_api.repository_api
  to   = azurerm_api_management_api.legacy_repository_api
}

data "azurerm_api_management_api" "legacy_repository_api" {
  provider = azurerm.api_management

  name                = var.repository_api.apim_api_name
  api_management_name = data.azurerm_api_management.legacy_platform.name
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name

  revision = var.repository_api.apim_api_revision
}
