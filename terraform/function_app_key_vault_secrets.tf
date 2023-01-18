resource "azurerm_key_vault_secret" "functionapp_host_key_secret" {
  name         = format("%s-hostkey", azurerm_linux_function_app.app.name)
  value        = data.azurerm_function_app_host_keys.app.primary_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [
    azurerm_role_assignment.deploy_principal_kv_role_assignment
  ]
}
