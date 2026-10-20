variable "subscription_id" {
  type        = string
  description = "Azure subscription that will host the environment."
}

variable "location" {
  type        = string
  description = "Azure region for every resource."
  default     = "westeurope"
}

variable "resource_group_name" {
  type        = string
  description = "Resource group that Terraform creates and owns."
  default     = "nakama-dev"
}

variable "app_service_name" {
  type        = string
  description = "App Service name. It is part of the public host name, so it must be unique across Azure."
  default     = "nakama-appservice"
}

variable "app_service_plan_name" {
  type        = string
  description = "App Service plan name."
  default     = "nakama-appserviceplan"
}

variable "postgresql_name" {
  type        = string
  description = "PostgreSQL server name. It is part of the server host name, so it must be unique across Azure."
  default     = "nakama-postgresql"
}

variable "app_service_sku" {
  type        = string
  description = "App Service plan SKU."
  default     = "B1"
}

variable "postgres_sku" {
  type        = string
  description = "PostgreSQL flexible server SKU."
  default     = "B_Standard_B1ms"
}

variable "postgres_version" {
  type        = string
  description = "PostgreSQL major version."
  default     = "16"
}

variable "postgres_storage_mb" {
  type        = number
  description = "PostgreSQL storage in MB."
  default     = 32768
}

variable "postgres_admin_login" {
  type        = string
  description = "PostgreSQL administrator login."
  default     = "nakamaadmin"
}

variable "postgres_admin_password" {
  type        = string
  description = "PostgreSQL administrator password. Pass it with TF_VAR_postgres_admin_password."
  sensitive   = true
}

variable "database_name" {
  type        = string
  description = "Application database name."
  default     = "nakama"
}

variable "tags" {
  type        = map(string)
  description = "Tags applied to every resource."
  default = {
    project     = "nakama"
    environment = "dev"
  }
}
