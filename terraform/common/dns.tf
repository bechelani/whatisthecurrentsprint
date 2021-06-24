# Create a DNS zone within Azure DNS
resource "azurerm_dns_zone" "public" {
  name                = "whatisthecurrentsprint.com"
  resource_group_name = azurerm_resource_group.common.name

  # Ignore number_of_record_sets as they could be updated using
  # other modules
  lifecycle {
    ignore_changes = [number_of_record_sets]
  }

  tags = var.tags
}
