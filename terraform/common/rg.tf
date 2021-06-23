# Create a resource group
resource "azurerm_resource_group" "common" {
  name     = "rg_witcs_common"
  location = "West US"

  tags = var.tags

  lifecycle {
    prevent_destroy = true
  }
}
