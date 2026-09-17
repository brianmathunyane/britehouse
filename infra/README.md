# Infrastructure (Terraform)

Provisions the Azure environment for the technical assessment.

## Resources created

| Resource | Purpose |
|---|---|
| `azurerm_resource_group` | Container for everything below. |
| `azurerm_storage_account` | Platform dependency for the Function App only (code package + host/trigger bookkeeping) — not used by business logic. |
| `azurerm_service_plan` (Y1, Linux) | Consumption plan hosting the Function App. |
| `azurerm_linux_function_app` | Runs the `OrdersXmlToJson` function ([`../function-app`](../function-app)), .NET 8 isolated worker. |
| `azurerm_logic_app_workflow` | Consumption Logic App shell. The actual Compose (sample XML) → HTTP (POST to the Function) workflow is authored in the Logic Apps Designer — Terraform's native `azurerm_logic_app_*` resources only cover a narrow set of trigger/action types, so hand-authoring the workflow definition in Terraform would be more brittle than using the designer. |

## Prerequisites

- Terraform >= 1.9
- Azure CLI, logged in (`az login`) with the target subscription selected (`az account set --subscription <id>`)
- Contributor access on that subscription (or the target resource group)

## Usage

```bash
cd infra
cp terraform.tfvars.example terraform.tfvars   # adjust project_name/location if needed
terraform init
terraform plan
terraform apply
```

State is kept locally (no remote backend configured) — appropriate for a
single-operator assessment; a real project would use an `azurerm` backend
(state in a Storage Account) instead.

## Outputs

After `apply`, `terraform output` exposes:

- `function_app_name` — used to deploy code (`func azure functionapp publish <name>`, see [`../function-app/README.md`](../function-app/README.md))
- `function_app_orders_endpoint` — the full `https://.../api/orders` URL (append `?code=<function-key>` to call it)
- `resource_group_name`, `storage_account_name`, `logic_app_name`

## Teardown

```bash
terraform destroy
```
