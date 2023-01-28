resource "azurerm_key_vault_secret" "service_bus_connection_string_secret" {
  name         = format("%s-connectionstring", azurerm_servicebus_namespace.ingest.name)
  value        = azurerm_servicebus_namespace_authorization_rule.event_ingest_func_app.primary_connection_string
  key_vault_id = azurerm_key_vault.kv.id
}
