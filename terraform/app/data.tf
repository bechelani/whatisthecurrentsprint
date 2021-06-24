data "azurerm_dns_zone" "witcs" {
  name                = "whatisthecurrentsprint.com"
  resource_group_name = "rg_witcs_common"
}

data "azurerm_container_registry" "acr" {
  name                = "mbbacr"
  resource_group_name = "rg_witcs_common"
}
