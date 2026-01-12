resource "azurerm_role_assignment" "app-to-storage" {
  scope                = azurerm_storage_account.function_app_storage.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = local.event_ingest_identity.principal_id
}

resource "azurerm_role_assignment" "app-to-servicebus-receiver" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = local.event_ingest_identity.principal_id
}

resource "azurerm_role_assignment" "app-to-servicebus-sender" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = local.event_ingest_identity.principal_id
}
