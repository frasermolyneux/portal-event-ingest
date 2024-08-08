output "subscription_id" {
  value = var.subscription_id
}

output "app_insights_name" {
  value = data.azurerm_application_insights.core.name
}

output "app_insights_resource_group" {
  value = data.azurerm_application_insights.core.resource_group_name
}

output "function_app_name" {
  value = azurerm_linux_function_app.app.name
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "staging_dashboard_name" {
  value = var.environment == "dev" ? azurerm_portal_dashboard.staging_dashboard[0].name : ""
}

output "dashboard_replacements" {
  value = local.dashboard_replacements
}
