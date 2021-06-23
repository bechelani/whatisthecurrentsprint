output "acr-login_server" {
  value       = azurerm_container_registry.acr.login_server
  description = "The login server for the ACR"
}

output "acr-admin_username" {
  value       = azurerm_container_registry.acr.admin_username
  description = "The admin user name for the ACR"
}

output "acr-admin_password" {
  value       = azurerm_container_registry.acr.admin_password
  description = "The admin password for the ACR"
  sensitive   = true
}

output "cosmos_db-endpoint" {
  value       = azurerm_cosmosdb_account.acc.endpoint
  description = "The endpoint used to connect the Comsos DB account"
}

output "cosmos_db-primary_readonly_key" {
  value       = azurerm_cosmosdb_account.acc.primary_readonly_key
  description = "The primary read-only master key for the Cosmos DB account"
  sensitive   = true
}
