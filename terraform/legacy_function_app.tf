resource "azurerm_linux_function_app" "legacy_app" {
  name = local.legacy_function_app_name
  tags = var.tags

  resource_group_name = azurerm_resource_group.legacy_rg.name
  location            = azurerm_resource_group.legacy_rg.location

  service_plan_id = data.azurerm_service_plan.core.id

  storage_account_name          = azurerm_storage_account.legacy_function_app_storage.name
  storage_uses_managed_identity = true

  https_only = true

  functions_extension_version = "~4"

  identity {
    type         = "UserAssigned"
    identity_ids = [local.event_ingest_funcapp_identity.id]
  }

  key_vault_reference_identity_id = local.event_ingest_funcapp_identity.id

  site_config {
    application_stack {
      use_dotnet_isolated_runtime = true
      dotnet_version              = "9.0"
    }

    cors {
      allowed_origins = ["https://portal.azure.com"]
    }

    application_insights_connection_string = data.azurerm_application_insights.core.connection_string
    application_insights_key               = data.azurerm_application_insights.core.instrumentation_key

    api_management_api_id = azurerm_api_management_api.legacy_event_ingest_api_versioned["v1"].id

    ftps_state          = "Disabled"
    always_on           = true
    minimum_tls_version = "1.2"

    health_check_path                 = "/api/health"
    health_check_eviction_time_in_min = 5
  }

  app_settings = {
    "AzureAppConfiguration__Endpoint"                = local.app_configuration_endpoint
    "AzureAppConfiguration__ManagedIdentityClientId" = local.event_ingest_funcapp_identity.client_id
    "AzureAppConfiguration__Environment"             = var.environment

    "AZURE_CLIENT_ID" = local.event_ingest_funcapp_identity.client_id

    "WEBSITE_RUN_FROM_PACKAGE"                      = "0" # This will be set to 0 on initial creation but will be updated to 1 when the package is deployed (required for azurerm_function_app_host_keys)
    "ApplicationInsightsAgent_EXTENSION_VERSION"    = "~3"
    "ServiceBusConnection__fullyQualifiedNamespace" = format("%s.servicebus.windows.net", azurerm_servicebus_namespace.legacy_ingest.name)
    "ServiceBusConnection__ManagedIdentityClientId" = local.event_ingest_funcapp_identity.client_id

    "RepositoryApi__BaseUrl"             = format("%s/repository", local.core_api_management.gateway_url)
    "RepositoryApi__ApiKey"              = format("@Microsoft.KeyVault(VaultName=%s;SecretName=%s)", azurerm_key_vault.legacy_kv.name, azurerm_key_vault_secret.legacy_repository_api_subscription_secret_primary.name)
    "RepositoryApi__ApplicationAudience" = var.repository_api.application_audience

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
  name                = azurerm_linux_function_app.legacy_app.name
  resource_group_name = azurerm_resource_group.legacy_rg.name
}
