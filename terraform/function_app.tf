resource "azurerm_linux_function_app" "app" {
  name = local.function_app_name
  tags = var.tags

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  service_plan_id = data.azurerm_service_plan.core.id

  storage_account_name          = azurerm_storage_account.function_app_storage.name
  storage_uses_managed_identity = true

  https_only = true

  functions_extension_version = "~4"

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      use_dotnet_isolated_runtime = true
      dotnet_version              = "8.0"
    }

    cors {
      allowed_origins = ["https://portal.azure.com"]
    }

    application_insights_connection_string = data.azurerm_application_insights.core.connection_string
    application_insights_key               = data.azurerm_application_insights.core.instrumentation_key

    api_management_api_id = azurerm_api_management_api.event_ingest_api.id

    ftps_state = "Disabled"
    //always_on           = true // Always on is not supported for consumption function apps
    minimum_tls_version = "1.2"

    //health_check_path                 = "/api/health" // Disabling health check for consumption function app
    //health_check_eviction_time_in_min = 5 // Disabling health check for consumption function app
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"                          = "0" # This will be set to 0 on initial creation but will be updated to 1 when the package is deployed (required for azurerm_function_app_host_keys)
    "ApplicationInsightsAgent_EXTENSION_VERSION"        = "~3"
    "service_bus_connection_string"                     = format("@Microsoft.KeyVault(VaultName=%s;SecretName=%s)", azurerm_key_vault.kv.name, azurerm_key_vault_secret.service_bus_connection_string_secret.name)
    "apim_base_url"                                     = data.azurerm_api_management.core.gateway_url
    "portal_repository_apim_subscription_key_primary"   = format("@Microsoft.KeyVault(VaultName=%s;SecretName=%s)", azurerm_key_vault.kv.name, azurerm_key_vault_secret.repository_api_subscription_secret_primary.name)
    "portal_repository_apim_subscription_key_secondary" = format("@Microsoft.KeyVault(VaultName=%s;SecretName=%s)", azurerm_key_vault.kv.name, azurerm_key_vault_secret.repository_api_subscription_secret_secondary.name)
    "repository_api_application_audience"               = var.repository_api.application_audience
    "repository_api_path_prefix"                        = var.repository_api.apim_path_prefix

    // https://learn.microsoft.com/en-us/azure/azure-monitor/profiler/profiler-azure-functions#app-settings-for-enabling-profiler
    "APPINSIGHTS_PROFILERFEATURE_VERSION"  = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION" = "~3"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_RUN_FROM_PACKAGE"] # Ignore changes to this property as it will be updated by the deployment pipeline
    ]
  }
}

data "azurerm_function_app_host_keys" "app" {
  name                = azurerm_linux_function_app.app.name
  resource_group_name = azurerm_resource_group.rg.name
}
