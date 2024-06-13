moved {
  from = azurerm_api_management_named_value.functionapp_host_key_named_value
  to   = azurerm_api_management_named_value.legacy_functionapp_host_key_named_value
}

resource "azurerm_api_management_named_value" "legacy_functionapp_host_key_named_value" {
  provider            = azurerm.api_management
  name                = azurerm_key_vault_secret.functionapp_host_key_secret.name
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

  display_name = azurerm_key_vault_secret.functionapp_host_key_secret.name

  secret = true

  value_from_key_vault {
    secret_id = azurerm_key_vault_secret.functionapp_host_key_secret.id
  }

  depends_on = [
    azurerm_role_assignment.legacy_apim_kv_role_assignment
  ]
}

moved {
  from = azurerm_api_management_backend.api_management_backend
  to   = azurerm_api_management_backend.legacy_api_management_backend
}

resource "azurerm_api_management_backend" "legacy_api_management_backend" {
  provider            = azurerm.api_management
  name                = local.workload_name
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

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
      "code" = format("{{${azurerm_api_management_named_value.legacy_functionapp_host_key_named_value.name}}}")
    }
  }
}

moved {
  from = azurerm_api_management_named_value.active_backend_named_value
  to   = azurerm_api_management_named_value.legacy_active_backend_named_value
}

resource "azurerm_api_management_named_value" "legacy_active_backend_named_value" {
  provider            = azurerm.api_management
  name                = "event-ingest-api-active-backend"
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

  secret = false

  display_name = "event-ingest-api-active-backend"
  value        = azurerm_api_management_backend.legacy_api_management_backend.name
}

moved {
  from = azurerm_api_management_named_value.audience_named_value
  to   = azurerm_api_management_named_value.legacy_audience_named_value
}

resource "azurerm_api_management_named_value" "legacy_audience_named_value" {
  provider            = azurerm.api_management
  name                = "event-ingest-api-audience"
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

  secret = false

  display_name = "event-ingest-api-audience"
  value        = format("api://%s", local.app_registration_name)
}

moved {
  from = azurerm_api_management_api.event_ingest_api
  to   = azurerm_api_management_api.legacy_event_ingest_api
}

resource "azurerm_api_management_api" "legacy_event_ingest_api" {
  provider            = azurerm.api_management
  name                = "event-ingest-api"
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

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

moved {
  from = azurerm_api_management_api_policy.example
  to   = azurerm_api_management_api_policy.legacy_event_ingest_api_policy
}

resource "azurerm_api_management_api_policy" "legacy_event_ingest_api_policy" {
  provider            = azurerm.api_management
  api_name            = azurerm_api_management_api.legacy_event_ingest_api.name
  resource_group_name = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name = data.azurerm_api_management.legacy_platform.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="{{event-ingest-api-active-backend}}" />
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="{{tenant-login-url}}{{tenant-id}}/v2.0/.well-known/openid-configuration" />
          <audiences>
              <audience>{{event-ingest-api-audience}}</audience>
          </audiences>
          <issuers>
              <issuer>https://sts.windows.net/{{tenant-id}}/</issuer>
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
    azurerm_api_management_named_value.legacy_active_backend_named_value,
    azurerm_api_management_named_value.legacy_audience_named_value
  ]
}

moved {
  from = azurerm_api_management_api_diagnostic.example
  to   = azurerm_api_management_api_diagnostic.legacy_event_ingest_api_diagnostic
}

resource "azurerm_api_management_api_diagnostic" "legacy_event_ingest_api_diagnostic" {
  provider                 = azurerm.api_management
  identifier               = "applicationinsights"
  api_name                 = azurerm_api_management_api.legacy_event_ingest_api.name
  resource_group_name      = data.azurerm_api_management.legacy_platform.resource_group_name
  api_management_name      = data.azurerm_api_management.legacy_platform.name
  api_management_logger_id = azurerm_api_management_logger.legacy_api_management_logger.id

  sampling_percentage = 100

  always_log_errors = true
  log_client_ip     = true

  verbosity = "information"

  http_correlation_protocol = "W3C"
}
