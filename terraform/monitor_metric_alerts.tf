resource "azurerm_monitor_scheduled_query_rules_alert" "on_chat_message_missing" {
  count = var.environment == "prd" ? 1 : 0

  name                = "portal-event-ingest-${var.environment} - OnChatMessage custom event - missing"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  description = "Triggers when the OnChatMessage custom event has not been ingested for six hours."

  frequency   = 60
  time_window = 360

  severity = 0
  enabled  = true

  data_source_id          = data.azurerm_application_insights.core.id
  authorized_resource_ids = [data.azurerm_application_insights.core.id]

  query = <<-QUERY
    customEvents
    | where name == "OnChatMessage"
    | where timestamp >= ago(6h)
    | summarize Events = count()
    | project Events
  QUERY

  trigger {
    operator  = "LessThan"
    threshold = 1
  }

  action {
    action_group = [
      data.azurerm_monitor_action_group.critical.id,
    ]
  }

  tags = var.tags
}
