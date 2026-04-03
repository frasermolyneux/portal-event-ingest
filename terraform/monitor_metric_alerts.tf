resource "azurerm_monitor_scheduled_query_rules_alert" "on_chat_message_missing" {
  count = var.environment == "prd" ? 1 : 0

  name = "portal-event-ingest-${var.environment} - OnChatMessage custom event - missing"

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  description = "Triggers when the OnChatMessage custom event has not been ingested for six hours."

  frequency   = 60
  time_window = 360

  severity = 0
  enabled  = true

  data_source_id          = data.azurerm_application_insights.app_insights.id
  authorized_resource_ids = [data.azurerm_application_insights.app_insights.id]

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
      local.action_group_map.critical.id
    ]
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "dead_letter_messages" {
  name = "portal-event-ingest-${var.environment}-dead-letter-messages"

  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [azurerm_servicebus_namespace.sb.id]

  description = "Triggers when dead-lettered messages exceed threshold across any queue."
  severity    = 2
  frequency   = "PT5M"
  window_size = "PT15M"
  enabled     = true

  criteria {
    metric_namespace = "Microsoft.ServiceBus/namespaces"
    metric_name      = "DeadletteredMessages"
    aggregation      = "Maximum"
    operator         = "GreaterThan"
    threshold        = 10
  }

  action {
    action_group_id = local.action_group_map.high.id
  }

  tags = var.tags
}

resource "azurerm_monitor_metric_alert" "queue_backlog" {
  name = "portal-event-ingest-${var.environment}-queue-backlog"

  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [azurerm_servicebus_namespace.sb.id]

  description = "Triggers when active messages exceed threshold, indicating processors cannot keep up."
  severity    = 2
  frequency   = "PT5M"
  window_size = "PT15M"
  enabled     = true

  criteria {
    metric_namespace = "Microsoft.ServiceBus/namespaces"
    metric_name      = "ActiveMessages"
    aggregation      = "Maximum"
    operator         = "GreaterThan"
    threshold        = 1000
  }

  action {
    action_group_id = local.action_group_map.high.id
  }

  tags = var.tags
}

resource "azurerm_monitor_scheduled_query_rules_alert" "processor_failure_rate" {
  count = var.environment == "prd" ? 1 : 0

  name = "portal-event-ingest-${var.environment} - Queue processor failure rate elevated"

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  description = "Triggers when queue processor functions have an elevated failure rate."

  frequency   = 5
  time_window = 5

  severity = 1
  enabled  = true

  data_source_id          = data.azurerm_application_insights.app_insights.id
  authorized_resource_ids = [data.azurerm_application_insights.app_insights.id]

  query = <<-QUERY
    requests
    | where success == false
    | where name startswith "Process"
    | summarize failureCount = count()
  QUERY

  trigger {
    operator  = "GreaterThan"
    threshold = 50
  }

  action {
    action_group = [
      local.action_group_map.high.id
    ]
  }

  tags = var.tags
}
