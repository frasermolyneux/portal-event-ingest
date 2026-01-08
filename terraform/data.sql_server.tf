data "azurerm_sql_server" "sql_server" {
  name                = local.workload_backend.sql_server.name
  resource_group_name = local.workload_backend.sql_server.resource_group_name
}
