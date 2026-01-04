locals {
  legacy_input = file("dashboards/dashboard.json")

  legacy_dashboard_replacements = {
    "subscription_id"          = var.subscription_id
    "resource_group_name"      = azurerm_resource_group.legacy_rg.name
    "key_vault_name"           = azurerm_key_vault.legacy_kv.name
    "function_app_name"        = azurerm_linux_function_app.legacy_app.name
    "core_resource_group_name" = data.azurerm_resource_group.core.name
    "app_insights_name"        = data.azurerm_application_insights.core.name
    "service_bus_name"         = azurerm_servicebus_namespace.legacy_ingest.name
    "api_management_name"      = data.azurerm_api_management.core.name
  }

  legacy_out = join("\n", [
    for line in split("\n", local.legacy_input) :
    format(
      replace(line, "/{(${join("|", keys(local.legacy_dashboard_replacements))})}/", "%s"),
      [
        for value in flatten(regexall("{(${join("|", keys(local.legacy_dashboard_replacements))})}", line)) :
        lookup(local.legacy_dashboard_replacements, value)
      ]...
    )
  ])
}

resource "azurerm_portal_dashboard" "legacy_app" {
  name = local.legacy_dashboard_name

  resource_group_name = azurerm_resource_group.legacy_rg.name
  location            = azurerm_resource_group.legacy_rg.location

  tags = var.tags

  dashboard_properties = local.legacy_out
}

resource "azurerm_portal_dashboard" "legacy_staging_dashboard" {
  count = var.environment == "dev" ? 1 : 0
  name  = "${local.legacy_dashboard_name}-staging"

  resource_group_name = azurerm_resource_group.legacy_rg.name
  location            = azurerm_resource_group.legacy_rg.location

  tags = var.tags

  dashboard_properties = local.legacy_out

  lifecycle {
    ignore_changes = [
      dashboard_properties
    ]
  }
}

moved {
  from = azurerm_portal_dashboard.app
  to   = azurerm_portal_dashboard.legacy_app
}

moved {
  from = azurerm_portal_dashboard.staging_dashboard
  to   = azurerm_portal_dashboard.legacy_staging_dashboard
}
