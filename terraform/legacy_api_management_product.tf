

// Shared API Management product/version set moved to portal-core.
// Drop legacy state without deleting the remote objects.

removed {
  from = azurerm_api_management_api_version_set.legacy_event_ingest_api

  lifecycle {
    destroy = false
  }
}

removed {
  from = azurerm_api_management_product.legacy_event_ingest_api

  lifecycle {
    destroy = false
  }
}

removed {
  from = azurerm_api_management_product_policy.legacy_event_ingest_api

  lifecycle {
    destroy = false
  }
}

