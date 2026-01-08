data "azurerm_api_management" "apim" {
  name                = data.terraform_remote_state.portal_environments.outputs.api_management.name
  resource_group_name = data.terraform_remote_state.portal_environments.outputs.api_management.resource_group_name
}
