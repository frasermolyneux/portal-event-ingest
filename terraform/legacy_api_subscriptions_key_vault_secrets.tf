moved {
  from = azurerm_key_vault_secret.repository_api_subscription_secret_primary
  to   = azurerm_key_vault_secret.legacy_repository_api_subscription_secret_primary
}

resource "azurerm_key_vault_secret" "legacy_repository_api_subscription_secret_primary" {
  name         = format("%s-api-key-primary", azurerm_api_management_subscription.legacy_repository_api_subscription.display_name)
  value        = azurerm_api_management_subscription.legacy_repository_api_subscription.primary_key
  key_vault_id = azurerm_key_vault.kv.id
}

moved {
  from = azurerm_key_vault_secret.repository_api_subscription_secret_secondary
  to   = azurerm_key_vault_secret.legacy_repository_api_subscription_secret_secondary
}

resource "azurerm_key_vault_secret" "legacy_repository_api_subscription_secret_secondary" {
  name         = format("%s-api-key-secondary", azurerm_api_management_subscription.legacy_repository_api_subscription.display_name)
  value        = azurerm_api_management_subscription.legacy_repository_api_subscription.secondary_key
  key_vault_id = azurerm_key_vault.kv.id
}
