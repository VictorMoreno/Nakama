locals {
  external_resources = "D:\\home\\site\\external_resources"
}

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                          = var.postgresql_name
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = var.postgres_version
  sku_name                      = var.postgres_sku
  storage_mb                    = var.postgres_storage_mb
  administrator_login           = var.postgres_admin_login
  administrator_password        = var.postgres_admin_password
  public_network_access_enabled = true
  tags                          = var.tags

  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_service_plan" "main" {
  name                = var.app_service_plan_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Windows"
  sku_name            = var.app_service_sku
  tags                = var.tags
}

resource "azurerm_windows_web_app" "main" {
  name                = var.app_service_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = var.tags

  site_config {
    always_on           = true
    ftps_state          = "Disabled"
    http2_enabled       = true
    minimum_tls_version = "1.2"
    use_32_bit_worker   = false
    health_check_path   = "/health"

    health_check_eviction_time_in_min = 10

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v10.0"
    }
  }

  app_settings = {
    ASPNETCORE_ENVIRONMENT               = "Production"
    Storage__ChatbotDocumentsPath        = "${local.external_resources}\\chatbot-documents"
    Storage__ConversationAttachmentsPath = "${local.external_resources}\\conversation-attachments"
    Storage__ChatbotIconsPath            = "${local.external_resources}\\chatbot-icons"
    Storage__FormationIconsPath          = "${local.external_resources}\\formation-icons"
  }

  lifecycle {
    ignore_changes = [app_settings]
  }
}
