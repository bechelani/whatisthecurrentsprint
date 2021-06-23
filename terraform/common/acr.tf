# Create an Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                = "mbbacr"
  resource_group_name = azurerm_resource_group.common.name
  location            = azurerm_resource_group.common.location
  admin_enabled       = true
  sku                 = "Basic"

  tags = var.tags
}
