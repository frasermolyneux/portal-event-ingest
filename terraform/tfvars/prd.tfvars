environment = "prd"
location    = "uksouth"
instance    = "01"

subscription_id = "32444f38-32f4-409f-889c-8e8aa2b5b4d1"

api_management_subscription_id     = "903b6685-c12a-4703-ac54-7ec1ff15ca43"
api_management_resource_group_name = "rg-platform-apim-prd-uksouth-01"
api_management_name                = "apim-platform-prd-uksouth-ty7og2i6qpv3s"

web_apps_subscription_id       = "903b6685-c12a-4703-ac54-7ec1ff15ca43"
web_apps_resource_group_name   = "rg-platform-plans-prd-uksouth-01"
web_apps_app_service_plan_name = "plan-platform-prd-uksouth-01"

frontdoor_subscription_id     = "db34f572-8b71-40d6-8f99-f29a27612144"
frontdoor_resource_group_name = "rg-platform-frontdoor-prd-uksouth-01"
frontdoor_name                = "fd-platform-prd-et7nxqc67pqjy"

dns_subscription_id     = "db34f572-8b71-40d6-8f99-f29a27612144"
dns_resource_group_name = "rg-platform-dns-prd-uksouth-01"
dns_zone_name           = "xtremeidiots.dev"

log_analytics_subscription_id     = "d68448b0-9947-46d7-8771-baa331a3063a"
log_analytics_resource_group_name = "rg-platform-logging-prd-uksouth-01"
log_analytics_workspace_name      = "log-platform-prd-uksouth-01"

repository_api = {
  application_name     = "portal-repository-prd-01"
  application_audience = "api://portal-repository-prd-01"
  apim_api_name        = "repository-api"
  apim_api_revision    = "1"
  apim_path_prefix     = "repository"
}

tags = {
  Environment = "prd",
  Workload    = "portal-event-ingest",
  DeployedBy  = "GitHub-Terraform",
  Git         = "https://github.com/frasermolyneux/portal-event-ingest"
}
