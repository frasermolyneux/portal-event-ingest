//https://github.com/frasermolyneux/portal-core/blob/main/terraform/app_insights.tf
data "azurerm_application_insights" "core" {
  name                = local.core_app_insights.name
  resource_group_name = data.azurerm_resource_group.core.name
}
