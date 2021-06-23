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

output "dns-name_servers" {
  value       = azurerm_dns_zone.public.name_servers
  description = "A list of values that make up the NS record for the zone"
}
