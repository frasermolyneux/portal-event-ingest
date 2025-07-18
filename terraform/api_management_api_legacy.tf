// Legacy resource for API Management API without a version number in the path
// This is used to maintain compatibility with existing clients that do not include the version in the path

data "local_file" "event_ingest_openapi_legacy" {
  filename = "../openapi/openapi-legacy.json"
}

resource "azurerm_api_management_api" "event_ingest_api_legacy" {
  name                = "event-ingest-api-legacy"
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  revision     = "1"
  display_name = "Event Ingest API"
  description  = "API for event ingest"
  path         = "event-ingest"
  protocols    = ["https"]

  subscription_required = true

  version        = ""
  version_set_id = azurerm_api_management_api_version_set.event_ingest_api_version_set.id

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  import {
    content_format = "openapi+json"
    content_value  = data.local_file.event_ingest_openapi_legacy.content
  }
}

resource "azurerm_api_management_product_api" "event_ingest_api_legacy" {
  api_name   = azurerm_api_management_api.event_ingest_api_legacy.name
  product_id = azurerm_api_management_product.event_ingest_api_product.product_id

  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name
}

resource "azurerm_api_management_api_policy" "event_ingest_api_policy_legacy" {
  api_name            = azurerm_api_management_api.event_ingest_api_legacy.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="${azurerm_api_management_backend.event_ingest_api_management_backend_versioned["v1"].name}" />
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
    azurerm_api_management_backend.event_ingest_api_management_backend_versioned
  ]
}

resource "azurerm_api_management_api_diagnostic" "event_ingest_api_diagnostic_legacy" {
  identifier               = "applicationinsights"
  api_name                 = azurerm_api_management_api.event_ingest_api_legacy.name
  resource_group_name      = data.azurerm_api_management.core.resource_group_name
  api_management_name      = data.azurerm_api_management.core.name
  api_management_logger_id = format("%s/providers/Microsoft.ApiManagement/service/serviceValue/loggers/%s", data.azurerm_resource_group.core.id, data.azurerm_application_insights.core.name)

  sampling_percentage = 100

  always_log_errors = true
  log_client_ip     = true

  verbosity = "information"

  http_correlation_protocol = "W3C"
}
