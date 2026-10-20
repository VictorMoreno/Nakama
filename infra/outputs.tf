output "app_url" {
  description = "Public URL of the application."
  value       = "https://${azurerm_windows_web_app.main.default_hostname}"
}

output "app_name" {
  description = "App Service name, used by az webapp deploy."
  value       = azurerm_windows_web_app.main.name
}

output "resource_group_name" {
  description = "Resource group that holds the environment."
  value       = azurerm_resource_group.main.name
}

output "postgres_fqdn" {
  description = "PostgreSQL server host name."
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "connection_string_template" {
  description = "Value for the ConnectionStrings__Nakama app setting; replace the password placeholder."
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=${var.database_name};Username=${var.postgres_admin_login};Password=<password>;SSL Mode=Require"
}
