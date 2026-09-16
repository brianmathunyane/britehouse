using System.Net;
using Britehouse.OrdersFunction.Parsing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Britehouse.OrdersFunction.Functions;

public class ProcessOrdersFunction
{
    private readonly ILogger<ProcessOrdersFunction> _logger;

    public ProcessOrdersFunction(ILogger<ProcessOrdersFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Accepts an XML payload containing one or more &lt;Order&gt; elements (wrapped in an
    /// &lt;Orders&gt; root, as sent by the assessment's Logic App) and returns them as a JSON array.
    /// </summary>
    [Function("ProcessOrders")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req)
    {
        string xmlBody;
        using (var reader = new StreamReader(req.Body))
        {
            xmlBody = await reader.ReadToEndAsync();
        }

        try
        {
            var orders = OrderXmlParser.Parse(xmlBody);
            _logger.LogInformation("Parsed {Count} order(s) from XML payload.", orders.Count);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(orders);
            return response;
        }
        catch (OrderXmlValidationException ex)
        {
            _logger.LogWarning(ex, "Rejected invalid XML payload.");
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteAsJsonAsync(new { error = ex.Message });
            return badRequest;
        }
    }
}
