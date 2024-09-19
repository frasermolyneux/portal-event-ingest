output "function_app_name" {
  value = azurerm_linux_function_app.app.name
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "staging_dashboard_name" {
  value = var.environment == "dev" ? azurerm_portal_dashboard.staging_dashboard[0].name : ""
}
