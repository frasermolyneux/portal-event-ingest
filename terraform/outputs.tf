output "function_app_name" {
  value = azurerm_linux_function_app.app.name
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "dashboard_name" {
  value = azurerm_portal_dashboard.app.name
}

output "dashboard_properties" {
  value = data.azurerm_portal_dashboard.app.dashboard_properties
}
