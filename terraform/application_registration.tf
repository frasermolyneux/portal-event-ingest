resource "random_uuid" "app_role_event_generator" {
}

resource "azuread_application" "event_ingest_api" {
  display_name     = local.app_registration_name
  identifier_uris  = [format("api://%s", local.app_registration_name)]
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADMyOrg"

  app_role {
    allowed_member_types = ["Application"]
    description          = "Event generators can create player and server events"
    display_name         = "EventGenerator"
    enabled              = true
    id                   = random_uuid.app_role_event_generator.result
    value                = "EventGenerator"
  }
}

resource "azuread_service_principal" "repository_api_service_principal" {
  application_id               = azuread_application.event_ingest_api.application_id
  app_role_assignment_required = false

  owners = [
    data.azuread_client_config.current.object_id
  ]
}
