resource "azurerm_role_assignment" "legacy_apim_kv_role_assignment" {
  scope                = azurerm_key_vault.legacy_kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = local.core_api_management_identity.principal_id
}

