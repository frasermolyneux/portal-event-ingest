resource "azurerm_role_assignment" "legacy_app-to-storage" {
  scope                = azurerm_storage_account.legacy_function_app_storage.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azurerm_linux_function_app.legacy_app.identity[0].principal_id
}

resource "azurerm_role_assignment" "legacy_app-to-servicebus-receiver" {
  scope                = azurerm_servicebus_namespace.legacy_ingest.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_linux_function_app.legacy_app.identity.0.principal_id
}

resource "azurerm_role_assignment" "legacy_app-to-servicebus-sender" {
  scope                = azurerm_servicebus_namespace.legacy_ingest.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_linux_function_app.legacy_app.identity.0.principal_id
}

resource "azurerm_role_assignment" "legacy_web_app_kv_role_assignment" {
  scope                = azurerm_key_vault.legacy_kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_function_app.legacy_app.identity.0.principal_id
}

resource "azuread_app_role_assignment" "legacy_repository_api" {
  app_role_id         = data.azuread_service_principal.repository_api.app_roles[index(data.azuread_service_principal.repository_api.app_roles.*.display_name, "ServiceAccount")].id
  principal_object_id = azurerm_linux_function_app.legacy_app.identity.0.principal_id
  resource_object_id  = data.azuread_service_principal.repository_api.object_id
}
