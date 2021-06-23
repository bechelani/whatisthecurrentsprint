# Create a resource group
resource "azurerm_resource_group" "witcs" {
  name     = "rg_witcs"
  location = "West US"

  tags = var.tags
}
