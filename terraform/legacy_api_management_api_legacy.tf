// Legacy resource for API Management API without a version number in the path
// This is used to maintain compatibility with existing clients that do not include the version in the path

data "local_file" "event_ingest_openapi_versionless" {
  filename = "../openapi/openapi-noversion.json"
}

resource "azurerm_api_management_api" "legacy_event_ingest_api_versionless" {
  name                = "event-ingest-api-versionless"
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name

  revision     = "1"
  display_name = "Event Ingest API"
  description  = "API for event ingest"
  path         = "event-ingest"
  protocols    = ["https"]

  subscription_required = true

  version        = ""
  version_set_id = local.event_ingest_api_shared.version_set_id

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  import {
    content_format = "openapi+json"
    content_value  = data.local_file.event_ingest_openapi_versionless.content
  }
}

resource "azurerm_api_management_product_api" "legacy_event_ingest_api_versionless" {
  api_name   = azurerm_api_management_api.legacy_event_ingest_api_versionless.name
  product_id = local.event_ingest_api_shared.product_id

  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name
}

resource "azurerm_api_management_api_policy" "legacy_event_ingest_api_policy_versionless" {
  api_name            = azurerm_api_management_api.legacy_event_ingest_api_versionless.name
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="${azurerm_api_management_backend.legacy_event_ingest_api_management_backend_versioned["v1"].name}" />
      <!-- Correct path rewriting for legacy API - use v1 to match function expectations -->
      <set-variable name="rewriteUriTemplate" value="@("/api/v1" + context.Request.OriginalUrl.Path.Substring(context.Api.Path.Length))" />
      <rewrite-uri template="@((string)context.Variables["rewriteUriTemplate"])" />
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
  </outbound>
  <on-error />
</policies>
XML

  depends_on = [
    azurerm_api_management_backend.legacy_event_ingest_api_management_backend_versioned
  ]
}

resource "azurerm_api_management_api_diagnostic" "legacy_event_ingest_api_diagnostic_versionless" {
  identifier          = "applicationinsights"
  api_name            = azurerm_api_management_api.legacy_event_ingest_api_versionless.name
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name
  api_management_logger_id = format(
    "%s/providers/Microsoft.ApiManagement/service/%s/loggers/%s",
    data.azurerm_resource_group.core.id,
    local.core_api_management.name,
    local.core_app_insights.name
  )

  sampling_percentage = 20

  always_log_errors = true
  log_client_ip     = true

  verbosity = "information"

  http_correlation_protocol = "W3C"
}
