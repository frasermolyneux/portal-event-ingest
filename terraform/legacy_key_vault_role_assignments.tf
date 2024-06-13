moved {
  from = azurerm_role_assignment.apim_kv_role_assignment
  to   = azurerm_role_assignment.legacy_apim_kv_role_assignment
}

resource "azurerm_role_assignment" "legacy_apim_kv_role_assignment" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = data.azurerm_api_management.legacy_platform.identity.0.principal_id
}

moved {
  from = azurerm_role_assignment.web_app_kv_role_assignment
  to   = azurerm_role_assignment.legacy_web_app_kv_role_assignment
}

resource "azurerm_role_assignment" "legacy_web_app_kv_role_assignment" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_function_app.app.identity.0.principal_id
}
