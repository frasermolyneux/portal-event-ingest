resource "azurerm_servicebus_namespace" "ingest" {
  name = local.service_bus_name

  resource_group_name = data.azurerm_service_plan.plan.resource_group_name
  location            = data.azurerm_service_plan.plan.location
  tags                = var.tags

  sku = "Basic"
}

resource "azurerm_servicebus_queue" "player_connected" {
  name         = "player_connected_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}


resource "azurerm_servicebus_queue" "player_connected" {
  name         = "chat_message_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "player_connected" {
  name         = "map_vote_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "player_connected" {
  name         = "server_connected_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_queue" "player_connected" {
  name         = "map_change_queue"
  namespace_id = azurerm_servicebus_namespace.ingest.id
}

resource "azurerm_servicebus_namespace_authorization_rule" "event_ingest_func_app" {
  name         = "event-ingest-func-app"
  namespace_id = azurerm_servicebus_namespace.ingest.id

  listen = true
  send   = true
  manage = false
}
