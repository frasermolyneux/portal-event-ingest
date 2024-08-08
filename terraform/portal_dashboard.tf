locals {
  input = file("dashboards/dashboard.json")

  map = {
    "app_insights_id"             = data.azurerm_application_insights.core.id
    "app_insights_resource_group" = data.azurerm_application_insights.core.resource_group_name
    "function_app_name"           = azurerm_linux_function_app.app.name
  }

  out = join("\n", [
    for line in split("\n", local.input) :
    format(
      replace(line, "/{(${join("|", keys(local.map))})}/", "%s"),
      [
        for value in flatten(regexall("{(${join("|", keys(local.map))})}", line)) :
        lookup(local.map, value)
      ]...
    )
  ])
}


resource "azurerm_portal_dashboard" "app" {
  name = local.dashboard_name

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = local.out
}

resource "azurerm_portal_dashboard" "staging_dashboard" {
  count = var.environment == "dev" ? 1 : 0
  name  = local.dashboard_name + "-staging"

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = "{\"lenses\": {}, \"metadata\": {}}"

  lifecycle {
    ignore_changes = [
      dashboard_properties
    ]
  }
}
