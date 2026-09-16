output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "function_app_name" {
  value = azurerm_linux_function_app.orders.name
}

output "function_app_default_hostname" {
  value = azurerm_linux_function_app.orders.default_hostname
}

output "function_app_orders_endpoint" {
  description = "The full URL of the ProcessOrders HTTP trigger (append ?code=<function-key> to call it)."
  value       = "https://${azurerm_linux_function_app.orders.default_hostname}/api/orders"
}

output "storage_account_name" {
  value = azurerm_storage_account.function_storage.name
}

output "logic_app_name" {
  value = azurerm_logic_app_workflow.orders_trigger.name
}
