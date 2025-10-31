resource "azurerm_monitor_metric_alert" "on_chat_message_missing" {
  count = var.environment == "prd" ? 1 : 0

  name = "portal-event-ingest-${var.environment} - OnChatMessage custom event - missing"

  resource_group_name = azurerm_resource_group.rg.name
  scopes              = [data.azurerm_application_insights.core.id]

  description = "Triggers when the OnChatMessage custom event has not been ingested for six hours."

  frequency   = "PT1H"
  window_size = "PT6H"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "customEvents"
    aggregation      = "Count"
    operator         = "LessThan"
    threshold        = 1

    dimension {
      name     = "name"
      operator = "Include"
      values   = ["OnChatMessage"]
    }

    skip_metric_validation = false
  }

  severity = 0

  action {
    action_group_id = data.azurerm_monitor_action_group.critical.id
  }

  tags = var.tags
}
