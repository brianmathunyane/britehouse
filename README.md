# britehouse

This assessment demonstrates a simple Azure integration flow where an XML order payload is received by an Azure Function, validated, and transformed into JSON before being returned through an HTTP endpoint. The function app is built with .NET 8 using the isolated worker model, with the parsing and transformation logic separated from the HTTP trigger for cleaner testing and maintenance.

The Azure infrastructure is provisioned with Terraform in the `infra` folder and includes the required resource group, storage account, Linux Function App consumption plan, Function App, and Logic App workflow shell. The setup is intentionally minimal to keep the environment easy to deploy and review while still representing a realistic Azure-hosted integration. The purpose of the assessment is to show end-to-end delivery of a cloud-native API that ingests XML, converts it to JSON, and deploys the required Azure resources in a repeatable way.
