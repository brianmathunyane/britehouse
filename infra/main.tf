resource "random_id" "suffix" {
  byte_length = 3
}

locals {
  suffix    = random_id.suffix.hex
  name_base = lower(replace(var.project_name, "-", ""))

  resource_group_name   = "rg-${var.project_name}-${var.environment}"
  app_service_plan_name = "asp-${var.project_name}-${var.environment}"
  function_app_name     = "func-${var.project_name}-${local.suffix}"
  logic_app_name        = "logic-${var.project_name}-${var.environment}"

  # Storage account names must be 3-24 chars, lowercase alphanumeric only —
  # truncate the project name so the generated name can never exceed the limit.
  storage_account_name = "st${substr(local.name_base, 0, 10)}${local.suffix}"

  tags = merge(var.tags, { environment = var.environment })
}

resource "azurerm_resource_group" "this" {
  name     = local.resource_group_name
  location = var.location
  tags     = local.tags
}

# Platform dependency only (function code package + host/trigger bookkeeping) —
# not used directly by the ProcessOrders business logic.
resource "azurerm_storage_account" "function_storage" {
  name                     = local.storage_account_name
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags
}

resource "azurerm_service_plan" "this" {
  name                = local.app_service_plan_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = "Y1" # Consumption plan
  tags                = local.tags
}

resource "azurerm_linux_function_app" "orders" {
  name                       = local.function_app_name
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  service_plan_id            = azurerm_service_plan.this.id
  storage_account_name       = azurerm_storage_account.function_storage.name
  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }
  }

  app_settings = {
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
  }

  tags = local.tags
}

# Shell workflow only — the Compose (sample XML) and HTTP (POST to the Function)
# actions are authored in the Logic Apps Designer, since Terraform's native
# azurerm_logic_app_* resources only cover a handful of action/trigger types.
resource "azurerm_logic_app_workflow" "orders_trigger" {
  name                = local.logic_app_name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags                = local.tags
}
