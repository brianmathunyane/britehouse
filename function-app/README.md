# OrdersXmlToJson Function App

Azure Function App (.NET 8, isolated worker model). Accepts an XML payload of orders and returns the equivalent JSON array.

## Endpoint

`POST /api/orders`

- **Auth:** function key (`AuthorizationLevel.Function`). Locally this is disabled
  (Core Tools serves anonymously), the deployed function requires `?code=<function-key>`
  or an `x-functions-key` header.
- **Request body:** XML, either:
  - an `<Orders>` root containing one or more `<Order>` children, or
  - a single top-level `<Order>` element.

  Each `<Order>` requires `OrderId`, `CustomerName`, `Amount` (numeric), `Date`
  (parseable date/date-time). Element name matching is case-insensitive.
- **Response:**
  - `200 OK` with a JSON array of orders, e.g.:
    ```json
    [
      { "orderId": "ORD-001", "customerName": "Jane Dlamini", "amount": 149.99, "date": "2026-09-10T00:00:00Z" }
    ]
    ```
  - `400 Bad Request` with `{ "error": "<reason>" }` for empty/malformed XML, missing
    required fields, or unparseable Amount/Date values.

A sample request payload is in [`samples/orders-valid.xml`](samples/orders-valid.xml).

## Project layout

```
OrdersXmlToJson/            Function app project
  Functions/                HTTP-triggered function (thin — parses body, delegates, maps errors to responses)
  Parsing/                  OrderXmlParser — the actual XML→model logic, framework-independent
  Models/                   Order DTO
OrdersXmlToJson.Tests/      xUnit tests for OrderXmlParser (no Functions host required)
```


## Run locally

Requires the .NET 8 SDK and [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local).

```bash
cd OrdersXmlToJson
func start
```

Then, in another terminal:

```bash
curl -X POST http://localhost:7071/api/orders \
  -H "Content-Type: application/xml" \
  --data-binary @../samples/orders-valid.xml
```

## Run the tests

```bash
dotnet test OrdersXmlToJson.Tests/OrdersXmlToJson.Tests.csproj
```

