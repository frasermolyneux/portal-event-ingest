resource "azurerm_api_management_named_value" "legacy_functionapp_host_key_named_value" {
  name                = azurerm_key_vault_secret.legacy_functionapp_host_key_secret.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  display_name = azurerm_key_vault_secret.legacy_functionapp_host_key_secret.name

  secret = true

  value_from_key_vault {
    secret_id = azurerm_key_vault_secret.legacy_functionapp_host_key_secret.id
  }

  depends_on = [
    azurerm_role_assignment.legacy_apim_kv_role_assignment
  ]
}
