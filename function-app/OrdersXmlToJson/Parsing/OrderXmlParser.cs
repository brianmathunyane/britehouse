using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Britehouse.OrdersFunction.Models;

namespace Britehouse.OrdersFunction.Parsing;

/// <summary>
/// Parses an &lt;Orders&gt; XML payload (or a single top-level &lt;Order&gt;) into a list of <see cref="Order"/>.
/// Element name matching is case-insensitive and namespace-agnostic to tolerate minor
/// variations from upstream senders (e.g. the Logic App's Compose action).
/// </summary>
public static class OrderXmlParser
{
    public static List<Order> Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new OrderXmlValidationException("XML payload is empty.");
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml, LoadOptions.None);
        }
        catch (XmlException ex)
        {
            throw new OrderXmlValidationException($"XML payload is not well-formed: {ex.Message}", ex);
        }

        var root = doc.Root ?? throw new OrderXmlValidationException("XML payload has no root element.");

        var orderElements = IsOrderElement(root)
            ? new[] { root }
            : root.Elements().Where(IsOrderElement).ToArray();

        if (orderElements.Length == 0)
        {
            throw new OrderXmlValidationException("No <Order> elements found under the XML root.");
        }

        return orderElements.Select(ParseOrder).ToList();
    }

    private static bool IsOrderElement(XElement element) =>
        element.Name.LocalName.Equals("Order", StringComparison.OrdinalIgnoreCase);

    private static Order ParseOrder(XElement element)
    {
        var orderId = GetRequiredChildValue(element, "OrderId");
        var customerName = GetRequiredChildValue(element, "CustomerName");
        var amountRaw = GetRequiredChildValue(element, "Amount");
        var dateRaw = GetRequiredChildValue(element, "Date");

        if (!decimal.TryParse(amountRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            throw new OrderXmlValidationException($"Order '{orderId}' has a non-numeric Amount value: '{amountRaw}'.");
        }

        if (!DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
        {
            throw new OrderXmlValidationException($"Order '{orderId}' has an unparseable Date value: '{dateRaw}'.");
        }

        return new Order
        {
            OrderId = orderId,
            CustomerName = customerName,
            Amount = amount,
            Date = date
        };
    }

    private static string GetRequiredChildValue(XElement parent, string name)
    {
        var child = parent.Elements().FirstOrDefault(e => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (child is null || string.IsNullOrWhiteSpace(child.Value))
        {
            throw new OrderXmlValidationException($"Missing or empty '{name}' element in an <Order>.");
        }

        return child.Value.Trim();
    }
}
