using Britehouse.OrdersFunction.Parsing;
using Xunit;

namespace Britehouse.OrdersFunction.Tests;

public class OrderXmlParserTests
{
    private const string ValidOrdersXml = """
        <Orders>
          <Order>
            <OrderId>ORD-001</OrderId>
            <CustomerName>Jane Dlamini</CustomerName>
            <Amount>149.99</Amount>
            <Date>2026-09-10T00:00:00Z</Date>
          </Order>
          <Order>
            <OrderId>ORD-002</OrderId>
            <CustomerName>Sipho Nkosi</CustomerName>
            <Amount>75.50</Amount>
            <Date>2026-09-11T00:00:00Z</Date>
          </Order>
        </Orders>
        """;

    [Fact]
    public void Parse_MultipleOrders_ReturnsAllOrdersInDocumentOrder()
    {
        var orders = OrderXmlParser.Parse(ValidOrdersXml);

        Assert.Equal(2, orders.Count);
        Assert.Equal("ORD-001", orders[0].OrderId);
        Assert.Equal("Jane Dlamini", orders[0].CustomerName);
        Assert.Equal(149.99m, orders[0].Amount);
        Assert.Equal("ORD-002", orders[1].OrderId);
    }

    [Fact]
    public void Parse_SingleTopLevelOrder_IsAccepted()
    {
        const string xml = """
            <Order>
              <OrderId>ORD-100</OrderId>
              <CustomerName>Lerato Mokoena</CustomerName>
              <Amount>10</Amount>
              <Date>2026-01-01</Date>
            </Order>
            """;

        var orders = OrderXmlParser.Parse(xml);

        Assert.Single(orders);
        Assert.Equal("ORD-100", orders[0].OrderId);
    }

    [Fact]
    public void Parse_CaseInsensitiveElementNames_AreAccepted()
    {
        const string xml = """
            <orders>
              <order>
                <orderid>ORD-200</orderid>
                <customername>Thabo Molefe</customername>
                <amount>20.00</amount>
                <date>2026-02-02</date>
              </order>
            </orders>
            """;

        var orders = OrderXmlParser.Parse(xml);

        Assert.Single(orders);
        Assert.Equal("ORD-200", orders[0].OrderId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_EmptyPayload_Throws(string xml)
    {
        Assert.Throws<OrderXmlValidationException>(() => OrderXmlParser.Parse(xml));
    }

    [Fact]
    public void Parse_MalformedXml_ThrowsValidationException()
    {
        const string xml = "<Orders><Order><OrderId>ORD-1</Order></Orders>";

        Assert.Throws<OrderXmlValidationException>(() => OrderXmlParser.Parse(xml));
    }

    [Fact]
    public void Parse_NoOrderElements_Throws()
    {
        const string xml = "<Orders><Note>nothing here</Note></Orders>";

        Assert.Throws<OrderXmlValidationException>(() => OrderXmlParser.Parse(xml));
    }

    [Fact]
    public void Parse_MissingRequiredField_Throws()
    {
        const string xml = """
            <Orders>
              <Order>
                <OrderId>ORD-1</OrderId>
                <Amount>10</Amount>
                <Date>2026-01-01</Date>
              </Order>
            </Orders>
            """;

        var ex = Assert.Throws<OrderXmlValidationException>(() => OrderXmlParser.Parse(xml));
        Assert.Contains("CustomerName", ex.Message);
    }


}
