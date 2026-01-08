resource "azurerm_api_management_api_version_set" "legacy_event_ingest_api_version_set" {
  name                = "event-ingest-api"
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name

  display_name      = "Event Ingest API"
  versioning_scheme = "Segment"
}

resource "azurerm_api_management_product" "legacy_event_ingest_api_product" {
  product_id          = "event-ingest-api"
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name

  display_name = "Event Ingest API"

  subscription_required = true
  approval_required     = false
  published             = true
}

resource "azurerm_api_management_product_policy" "legacy_event_ingest_api_product_policy" {
  product_id          = azurerm_api_management_product.legacy_event_ingest_api_product.product_id
  resource_group_name = local.core_api_management.resource_group_name
  api_management_name = local.core_api_management.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" />
      <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="JWT validation was unsuccessful" require-expiration-time="true" require-scheme="Bearer" require-signed-tokens="true">
          <openid-config url="https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}/v2.0/.well-known/openid-configuration" />
          <audiences>
          <audience>${local.event_ingest_api_identifier_uri}</audience>
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
}

import {
  to = azurerm_api_management_api_version_set.legacy_event_ingest_api_version_set
  id = "${local.core_api_management.id}/apiVersionSets/event-ingest-api"
}

import {
  to = azurerm_api_management_product.legacy_event_ingest_api_product
  id = "${local.core_api_management.id}/products/event-ingest-api"
}

import {
  to = azurerm_api_management_product_policy.legacy_event_ingest_api_product_policy
  id = "${local.core_api_management.id}/products/event-ingest-api"
}

