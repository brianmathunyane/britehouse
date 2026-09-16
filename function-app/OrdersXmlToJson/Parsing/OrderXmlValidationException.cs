namespace Britehouse.OrdersFunction.Parsing;

/// <summary>Thrown when the incoming XML is malformed or doesn't match the expected Orders/Order shape.</summary>
public class OrderXmlValidationException : Exception
{
    public OrderXmlValidationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
