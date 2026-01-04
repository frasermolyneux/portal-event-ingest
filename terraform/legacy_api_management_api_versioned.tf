// This file contains configuration for dynamically importing versioned APIs
// based on the OpenAPI specs captured during the build process

locals {
  // List of version files that exist (excluding legacy which is handled separately)
  legacy_version_files = fileset("../openapi", "openapi-v*.json")

  // Extract version strings from filenames (e.g., "v1", "v1.1", "v2")
  legacy_version_strings = [for file in local.legacy_version_files :
    trimsuffix(trimprefix(basename(file), "openapi-"), ".json")
  ]

  // Filter out legacy as it's handled in separate file
  legacy_versioned_apis = [for version in local.legacy_version_strings :
    version if version != "legacy"
  ]

  // Extract major versions from all discovered APIs (v1, v2, etc.)
  legacy_major_versions = toset([for version in local.legacy_versioned_apis :
    regex("^(v[0-9]+)", version)[0]
  ])

  // Dynamic API version formatting - automatically add .0 for versions without dots
  legacy_api_version_formats = { for version in local.legacy_versioned_apis :
    version => can(regex("\\.", version)) ? version : "${version}.0"
  }

  // Static mapping of major versions to function app configurations
  // For event ingest, we only have one function app that handles all versions
  legacy_backend_mapping = {
    "v1" = {
      name         = local.legacy_function_app_name
      hostname     = azurerm_linux_function_app.legacy_app.default_hostname
      protocol     = "http"
      tls_validate = true
      description  = "Backend for v1.x APIs"
      exists       = true
    }
    # Add future versions with explicit entries here as needed
  }

  // Filter to only include function apps that have a major version in our discovered APIs
  legacy_filtered_backend_mapping = {
    for k, v in local.legacy_backend_mapping :
    k => v if contains(local.legacy_major_versions, k) && v.exists
  }

  // Default backend uses the lowest available major version (v1 in most cases)
  legacy_default_backend_version = length(local.legacy_filtered_backend_mapping) > 0 ? sort(keys(local.legacy_filtered_backend_mapping))[0] : "v1"
  legacy_default_backend         = length(local.legacy_filtered_backend_mapping) > 0 ? local.legacy_filtered_backend_mapping[local.legacy_default_backend_version] : local.legacy_backend_mapping["v1"]

  // Helper function to get the major version from a full version (e.g., "v1" from "v1.2")
  legacy_get_major_version = { for version in local.legacy_versioned_apis :
    version => regex("^(v[0-9]+)", version)[0]
  }

  // Get the backend configuration for a specific API version
  legacy_get_backend_for_version = { for version in local.legacy_versioned_apis :
    version => contains(keys(local.legacy_filtered_backend_mapping), local.legacy_get_major_version[version]) ?
    local.legacy_filtered_backend_mapping[local.legacy_get_major_version[version]] :
    local.legacy_default_backend
  }
}

// Data sources for versioned OpenAPI specification files
data "local_file" "event_ingest_openapi_versioned" {
  for_each = toset(local.legacy_versioned_apis)
  filename = "../openapi/openapi-${each.key}.json"
}

// Create backend for versioned APIs
resource "azurerm_api_management_backend" "legacy_event_ingest_api_management_backend_versioned" {
  for_each = local.legacy_filtered_backend_mapping

  name                = each.value.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  protocol    = lower(each.value.protocol)
  title       = each.value.name
  description = each.value.description
  url         = format("https://%s/api", each.value.hostname)

  tls {
    validate_certificate_chain = each.value.tls_validate
    validate_certificate_name  = each.value.tls_validate
  }

  credentials {
    query = {
      "code" = format("{{${azurerm_api_management_named_value.legacy_functionapp_host_key_named_value.name}}}")
    }
  }
}

// Dynamic versioned APIs that are discovered from OpenAPI spec files
resource "azurerm_api_management_api" "legacy_event_ingest_api_versioned" {
  for_each = toset(local.legacy_versioned_apis)

  name                = "event-ingest-api-${replace(each.key, ".", "-")}"
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  revision     = "1"
  display_name = "Event Ingest API"
  description  = "API for event ingest"
  path         = "event-ingest"
  protocols    = ["https"]

  subscription_required = true

  version        = each.key
  version_set_id = azurerm_api_management_api_version_set.legacy_event_ingest_api_version_set.id

  subscription_key_parameter_names {
    header = "Ocp-Apim-Subscription-Key"
    query  = "subscription-key"
  }

  import {
    content_format = "openapi+json"
    content_value  = data.local_file.event_ingest_openapi_versioned[each.key].content
  }
}

// Add versioned APIs to the product
resource "azurerm_api_management_product_api" "legacy_event_ingest_api_versioned" {
  for_each = azurerm_api_management_api.legacy_event_ingest_api_versioned

  api_name   = each.value.name
  product_id = azurerm_api_management_product.legacy_event_ingest_api_product.product_id

  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name
}

// Configure policies for versioned APIs
resource "azurerm_api_management_api_policy" "legacy_event_ingest_api_policy_versioned" {
  for_each = azurerm_api_management_api.legacy_event_ingest_api_versioned

  api_name            = each.value.name
  resource_group_name = data.azurerm_api_management.core.resource_group_name
  api_management_name = data.azurerm_api_management.core.name

  xml_content = <<XML
<policies>
  <inbound>
      <base/>
      <set-backend-service backend-id="${
  contains(keys(local.legacy_filtered_backend_mapping), local.legacy_get_major_version[each.key])
  ? azurerm_api_management_backend.legacy_event_ingest_api_management_backend_versioned[local.legacy_get_major_version[each.key]].name
  : azurerm_api_management_backend.legacy_event_ingest_api_management_backend_versioned[local.legacy_default_backend_version].name
}" />
      <!-- Correct path rewriting for versioned APIs -->
      <set-variable name="rewriteUriTemplate" value="@("/api" + context.Request.OriginalUrl.Path.Substring(context.Api.Path.Length))" />
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

// Configure diagnostics for versioned APIs
resource "azurerm_api_management_api_diagnostic" "legacy_event_ingest_api_diagnostic_versioned" {
  for_each = azurerm_api_management_api.legacy_event_ingest_api_versioned

  identifier               = "applicationinsights"
  api_name                 = each.value.name
  resource_group_name      = data.azurerm_api_management.core.resource_group_name
  api_management_name      = data.azurerm_api_management.core.name
  api_management_logger_id = format("%s/providers/Microsoft.ApiManagement/service/serviceValue/loggers/%s", data.azurerm_resource_group.core.id, data.azurerm_application_insights.core.name)

  sampling_percentage = 20

  always_log_errors = true
  log_client_ip     = true

  verbosity = "information"

  http_correlation_protocol = "W3C"
}

moved {
  from = azurerm_api_management_backend.event_ingest_api_management_backend_versioned
  to   = azurerm_api_management_backend.legacy_event_ingest_api_management_backend_versioned
}

moved {
  from = azurerm_api_management_api.event_ingest_api_versioned
  to   = azurerm_api_management_api.legacy_event_ingest_api_versioned
}

moved {
  from = azurerm_api_management_product_api.event_ingest_api_versioned
  to   = azurerm_api_management_product_api.legacy_event_ingest_api_versioned
}

moved {
  from = azurerm_api_management_api_policy.event_ingest_api_policy_versioned
  to   = azurerm_api_management_api_policy.legacy_event_ingest_api_policy_versioned
}

moved {
  from = azurerm_api_management_api_diagnostic.event_ingest_api_diagnostic_versioned
  to   = azurerm_api_management_api_diagnostic.legacy_event_ingest_api_diagnostic_versioned
}
