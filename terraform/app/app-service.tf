# Create an App Service Plan with Linux
resource "azurerm_app_service_plan" "asp" {
  name                = "witcs-asp"
  location            = azurerm_resource_group.witcs.location
  resource_group_name = azurerm_resource_group.witcs.name

  # Define Linux as Host OS
  kind     = "Linux"
  reserved = true # Mandatory for Linux plans

  # Choose size
  sku {
    tier = "Basic"
    size = "B1"
  }

  tags = var.tags
}

# Create an Azure Web App for Containers in that App Service Plan
resource "azurerm_app_service" "app" {
  name                = "witcs-app"
  location            = azurerm_resource_group.witcs.location
  resource_group_name = azurerm_resource_group.witcs.name
  app_service_plan_id = azurerm_app_service_plan.asp.id

  https_only = true

  # Do not attach Storage by default
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false

    # Settings to connect to the Cosmos DB
    CosmosDb__Account               = azurerm_cosmosdb_account.acc.endpoint
    CosmosDb__Key                   = azurerm_cosmosdb_account.acc.primary_readonly_key

    # Settings for private Container Registires
    DOCKER_REGISTRY_SERVER_URL      = data.azurerm_container_registry.acr.login_server
    DOCKER_REGISTRY_SERVER_USERNAME = data.azurerm_container_registry.acr.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = data.azurerm_container_registry.acr.admin_password
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|appsvcsample/static-site:latest"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Create a cname record for the app service in our dns zone
resource "azurerm_dns_cname_record" "witcs" {
  name                = "test"
  zone_name           = data.azurerm_dns_zone.witcs.name
  resource_group_name = data.azurerm_dns_zone.witcs.resource_group_name
  ttl                 = 300
  record              = azurerm_app_service.app.default_site_hostname
}

# Create a dns txt record for the app service in our dns zone
resource "azurerm_dns_txt_record" "witcs" {
  name                = "asuid.${azurerm_dns_cname_record.witcs.name}"
  zone_name           = data.azurerm_dns_zone.witcs.name
  resource_group_name = data.azurerm_dns_zone.witcs.resource_group_name
  ttl                 = 300

  record {
    value = azurerm_app_service.app.custom_domain_verification_id
  }
}

# Create a custom hostname binding for the App Service
resource "azurerm_app_service_custom_hostname_binding" "witcs" {
  hostname            = join(".", [azurerm_dns_cname_record.witcs.name, azurerm_dns_cname_record.witcs.zone_name])
  app_service_name    = azurerm_app_service.app.name
  resource_group_name = azurerm_app_service.app.resource_group_name

  # Ignore ssl_state and thumbprint as they are managed using
  # azurerm_app_service_certificate_binding.example
  lifecycle {
    ignore_changes = [ssl_state, thumbprint]
  }
}

# Create a managed certificate for our app service
resource "azurerm_app_service_managed_certificate" "witcs" {
  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.witcs.id

  tags = var.tags
}

resource "azurerm_app_service_certificate_binding" "witcs" {
  hostname_binding_id = azurerm_app_service_custom_hostname_binding.witcs.id
  certificate_id      = azurerm_app_service_managed_certificate.witcs.id
  ssl_state           = "SniEnabled"
}
