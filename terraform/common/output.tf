output "dns-name_servers" {
  value       = azurerm_dns_zone.public.name_servers
  description = "A list of values that make up the NS record for the zone"
}
