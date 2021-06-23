output "cosmos_db-endpoint" {
  value       = azurerm_cosmosdb_account.acc.endpoint
  description = "The endpoint used to connect the Comsos DB account"
}

output "cosmos_db-primary_readonly_key" {
  value       = azurerm_cosmosdb_account.acc.primary_readonly_key
  description = "The primary read-only master key for the Cosmos DB account"
  sensitive   = true
}
