resource "azurerm_role_assignment" "legacy_app-to-storage" {
  scope                = azurerm_storage_account.legacy_function_app_storage.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = local.event_ingest_funcapp_identity.principal_id
}

resource "azurerm_role_assignment" "legacy_app-to-servicebus-receiver" {
  scope                = azurerm_servicebus_namespace.legacy_ingest.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = local.event_ingest_funcapp_identity.principal_id
}

resource "azurerm_role_assignment" "legacy_app-to-servicebus-sender" {
  scope                = azurerm_servicebus_namespace.legacy_ingest.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = local.event_ingest_funcapp_identity.principal_id
}

resource "azurerm_role_assignment" "legacy_web_app_kv_role_assignment" {
  scope                = azurerm_key_vault.legacy_kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = local.event_ingest_funcapp_identity.principal_id
}
