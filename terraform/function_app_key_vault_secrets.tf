resource "azurerm_key_vault_secret" "functionapp_host_key_secret" {
  name         = format("%s-hostkey", azurerm_linux_function_app.legacy_app.name)
  value        = data.azurerm_function_app_host_keys.app.primary_key
  key_vault_id = azurerm_key_vault.kv.id
}
