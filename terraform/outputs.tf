output "function_app_name" {
  value = azurerm_linux_function_app.app.name
}

output "dashboard_properties" {
  value = data.azurerm_portal_dashboard.app.dashboard_properties
}
