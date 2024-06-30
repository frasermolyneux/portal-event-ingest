resource "azurerm_api_management_named_value" "functionapp_host_key_named_value" {
  name                = azurerm_key_vault_secret.functionapp_host_key_secret.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  display_name = azurerm_key_vault_secret.functionapp_host_key_secret.name

  secret = true

  value_from_key_vault {
    secret_id = azurerm_key_vault_secret.functionapp_host_key_secret.id
  }

  depends_on = [
    azurerm_role_assignment.apim_kv_role_assignment
  ]
}

resource "azurerm_api_management_backend" "api_management_backend" {
  name                = local.workload_name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  protocol    = "http"
  title       = local.workload_name
  description = local.workload_name
  url         = format("https://%s/api", azurerm_linux_function_app.app.default_hostname)

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }

  credentials {
    query = {
      "code" = format("{{${azurerm_api_management_named_value.functionapp_host_key_named_value.name}}}")
    }
  }
}

resource "azurerm_api_management_named_value" "active_backend_named_value" {
  name                = "event-ingest-api-active-backend"
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  secret = false

  display_name = "event-ingest-api-active-backend"
  value        = azurerm_api_management_backend.api_management_backend.name
}

resource "azurerm_api_management_named_value" "audience_named_value" {
  name                = "event-ingest-api-audience"
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  secret = false

  display_name = "event-ingest-api-audience"
  value        = format("api://%s", local.app_registration_name)
}

resource "azurerm_api_management_api" "event_ingest_api" {
  name                = "event-ingest-api"
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  revision     = "1"
  display_name = "Event Ingest API"
  description  = "API for event ingest"
  path         = "event-ingest"
  protocols    = ["https"]

  subscription_required = true

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  import {
    content_format = "openapi+json"
    content_value  = file("../EventIngest.openapi+json.json")
  }
}

resource "azurerm_api_management_api_policy" "event_ingest_api_policy" {
  api_name            = azurerm_api_management_api.event_ingest_api.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{event-ingest-api-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{event-ingest-api-audience}}</audience>
          </audiences>
          <issuers>
              <issuer>https://sts.windows.net/${data.azuread_client_config.current.tenant_id}/</issuer>
          </issuers>
          <required-claims>
              <claim name="roles" match="any">
                <value>EventGenerator</value>
              </claim>
          </required-claims>
      </validate-jwt>
  </inbound>
  <backend>
      <forward-request />
  </backend>
  <outbound>
      <base/>
      <cache-store duration="3600" />
  </outbound>
  <on-error />
</policies>
XML

  depends_on = [
    azurerm_api_management_named_value.active_backend_named_value,
    azurerm_api_management_named_value.audience_named_value
  ]
}

resource "azurerm_api_management_api_diagnostic" "event_ingest_api_diagnostic" {
  identifier               = "applicationinsights"
  api_name                 = azurerm_api_management_api.event_ingest_api.name
  resource_group_name      = data.azurerm_api_management.core.resource_group_name
  api_management_name      = data.azurerm_api_management.core.name
  api_management_logger_id = format("%s/providers/Microsoft.Insights/components/%s", data.azurerm_resource_group.core.id, data.azurerm_application_insights.core.name)

  sampling_percentage = 100

  always_log_errors = true
  log_client_ip     = true

  verbosity = "information"

  http_correlation_protocol = "W3C"
}
