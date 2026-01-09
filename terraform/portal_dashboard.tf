locals {
  input = file("dashboards/dashboard.json")

  dashboard_replacements = {
    "subscription_id"          = var.subscription_id
    "resource_group_name"      = data.azurerm_resource_group.rg.name
    "function_app_name"        = azurerm_linux_function_app.function_app.name
    "core_resource_group_name" = data.azurerm_api_management.api_management.resource_group_name
    "app_insights_name"        = data.azurerm_application_insights.app_insights.name
    "service_bus_name"         = azurerm_servicebus_namespace.sb.name
    "api_management_name"      = data.azurerm_api_management.api_management.name
  }

  out = join("\n", [
    for line in split("\n", local.input) :
    format(
      replace(line, "/{(${join("|", keys(local.dashboard_replacements))})}/", "%s"),
      [
        for value in flatten(regexall("{(${join("|", keys(local.dashboard_replacements))})}", line)) :
        lookup(local.dashboard_replacements, value)
      ]...
    )
  ])
}

resource "azurerm_portal_dashboard" "dashboard" {
  name = local.dashboard_name

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = local.out
}

resource "azurerm_portal_dashboard" "staging_dashboard" {
  count = var.environment == "dev" ? 1 : 0
  name  = "${local.dashboard_name}-staging"

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = local.out

  lifecycle {
    ignore_changes = [
      dashboard_properties
    ]
  }
}
