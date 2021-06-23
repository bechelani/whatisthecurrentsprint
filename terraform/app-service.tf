# Create an App Service Plan with Linux
resource "azurerm_app_service_plan" "asp" {
  name                = "witcs-asp"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  # Define Linux as Host OS
  kind     = "Linux"
  reserved = true # Mandatory for Linux plans

  # Choose size
  sku {
    tier = "Standard"
    size = "S1"
  }

  tags = {
    Owner        = "Michel Bechelani"
    Project      = "WhatIsTheCurrentSprint"
    Created_Date = "2021-06-22"
  }
}

# Create an Azure Web App for Containers in that App Service Plan
resource "azurerm_app_service" "appservice" {
  name                = "witcs-wa"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id

  # Do not attach Storage by default
  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false

    # Settings to connect to the Cosmos DB
    CosmosDb__Account               = azurerm_cosmosdb_account.acc.endpoint
    CosmosDb__Key                   = azurerm_cosmosdb_account.acc.primary_readonly_key

    # Settings for private Container Registires
    DOCKER_REGISTRY_SERVER_URL      = azurerm_container_registry.acr.login_server
    DOCKER_REGISTRY_SERVER_USERNAME = azurerm_container_registry.acr.admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD = azurerm_container_registry.acr.admin_password
  }

  # Configure Docker Image to load on start
  site_config {
    linux_fx_version = "DOCKER|appsvcsample/static-site:latest"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    Owner        = "Michel Bechelani"
    Project      = "WhatIsTheCurrentSprint"
    Created_Date = "2021-06-22"
  }
}
