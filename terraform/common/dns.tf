# Create a DNS zone within Azure DNS
resource "azurerm_dns_zone" "public" {
  name                = "whatisthecurrentsprint.com"
  resource_group_name = azurerm_resource_group.common.name

  tags = var.tags
}
