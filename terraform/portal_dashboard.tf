locals {
  tokens = {
    "app_insights_id"             = data.azurerm_application_insights.core.id
    "app_insights_resource_group" = data.azurerm_application_insights.core.resource_group_name
    "function_app_name"           = azurerm_linux_function_app.app.name
  }

  template = file("dashboards/dashboard.json")

  result = format(
    # Replace any variable with a %s which will be filled by the format func
    replace(local.template, "/{(${join("|", keys(local.tokens))})}/", "%s"),
    [
      # Replace each variable found in the given template with its value from the tokens map
      for value in flatten(regexall("{(${join("|", keys(local.tokens))})}", local.template)) :
      lookup(local.tokens, value)
    ]
  )
}


resource "azurerm_portal_dashboard" "app" {
  name = local.dashboard_name

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = local.result
}
