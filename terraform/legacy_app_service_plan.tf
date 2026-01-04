resource "azurerm_service_plan" "legacy_sp" {
  name                = local.legacy_app_service_plan_name
  location            = azurerm_resource_group.legacy_rg.location
  resource_group_name = azurerm_resource_group.legacy_rg.name

  os_type  = "Linux"
  sku_name = "FC1"
}

moved {
  from = azurerm_service_plan.sp
  to   = azurerm_service_plan.legacy_sp
}
