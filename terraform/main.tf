# We strongly recommend using the required_providers block to set the
# Azure Provider source and version being used
terraform {
  backend "local" {}
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=2.46.0"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Create a resource group
resource "azurerm_resource_group" "rg" {
  name     = "rg_witcs"
  location = "West US"

  tags = {
    Owner        = "Michel Bechelani"
    Project      = "WhatIsTheCurrentSprint"
    Created_Date = "2021-06-22"
  }
}
