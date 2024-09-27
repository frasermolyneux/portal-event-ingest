resource "azurerm_servicebus_namespace" "ingest" {
  name = local.service_bus_name

  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  tags                = var.tags

  sku = "Basic"

  local_auth_enabled = false
}

resource "azurerm_servicebus_queue" "player_connected" {
  name         = "player_connected_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "chat_message" {
  name         = "chat_message_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "map_vote" {
  name         = "map_vote_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "server_connected" {
  name         = "server_connected_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "map_change" {
  name         = "map_change_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}
