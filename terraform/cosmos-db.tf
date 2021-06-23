# Create an Azure Cosmos DB account
resource "azurerm_cosmosdb_account" "acc" {
  name                = "witcs-cosmosdb"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  offer_type                = "Standard"
  kind                      = "GlobalDocumentDB"
  enable_automatic_failover = false

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }

  tags = {
    Owner        = "Michel Bechelani"
    Project      = "WhatIsTheCurrentSprint"
    Created_Date = "2021-06-22"
  }
}

# Create an Cosmos DB database
resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "WITCS"
  resource_group_name = azurerm_cosmosdb_account.acc.resource_group_name
  account_name        = azurerm_cosmosdb_account.acc.name
}

# Create a collection in the created Cosmos DB database
resource "azurerm_cosmosdb_sql_container" "coll" {
  name                = "Sprints"
  resource_group_name = azurerm_cosmosdb_account.acc.resource_group_name
  account_name        = azurerm_cosmosdb_account.acc.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/id"
}
