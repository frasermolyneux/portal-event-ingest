resource "azurerm_servicebus_queue" "player_connected" {
  name         = "player_connected_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "chat_message" {
  name         = "chat_message_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "map_vote" {
  name         = "map_vote_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "server_connected" {
  name         = "server_connected_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "map_change" {
  name         = "map_change_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

# V2 Service Bus Queues
resource "azurerm_servicebus_queue" "v2_player_lifecycle" {
  name         = "v2_player_lifecycle_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "v2_communication" {
  name         = "v2_communication_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "v2_map_voting" {
  name         = "v2_map_voting_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}

resource "azurerm_servicebus_queue" "v2_server_game" {
  name         = "v2_server_game_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id
}
