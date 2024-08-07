resource "azurerm_portal_dashboard" "app" {
  name = local.dashboard_name

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  tags = var.tags

  dashboard_properties = file("dashboards/dashboard.json")
}
