resource "azurerm_servicebus_namespace" "legacy_ingest" {
  name = local.legacy_service_bus_name

  resource_group_name = azurerm_resource_group.legacy_rg.name
  location            = azurerm_resource_group.legacy_rg.location
  tags                = var.tags

  sku = "Basic"

  local_auth_enabled = false
}

resource "azurerm_servicebus_queue" "legacy_player_connected" {
  name         = "player_connected_queue"
  namespace_id = azurerm_servicebus_namespace.legacy_ingest.id
}

resource "azurerm_servicebus_queue" "legacy_chat_message" {
  name         = "chat_message_queue"
  namespace_id = azurerm_servicebus_namespace.legacy_ingest.id
}

resource "azurerm_servicebus_queue" "legacy_map_vote" {
  name         = "map_vote_queue"
  namespace_id = azurerm_servicebus_namespace.legacy_ingest.id
}

resource "azurerm_servicebus_queue" "legacy_server_connected" {
  name         = "server_connected_queue"
  namespace_id = azurerm_servicebus_namespace.legacy_ingest.id
}

resource "azurerm_servicebus_queue" "legacy_map_change" {
  name         = "map_change_queue"
  namespace_id = azurerm_servicebus_namespace.legacy_ingest.id
}
