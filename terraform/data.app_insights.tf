data "azurerm_application_insights" "app_insights" {
  name                = locals.app_insights.name
  resource_group_name = locals.app_insights.resource_group_name
}
