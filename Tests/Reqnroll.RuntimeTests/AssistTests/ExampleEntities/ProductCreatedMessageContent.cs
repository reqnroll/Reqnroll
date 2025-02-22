using System;

namespace Reqnroll.RuntimeTests.AssistTests.ExampleEntities;

public sealed class ProductCreatedMessageContent
{
    public ProductCreatedMessageContent(string productCode, string productName, DateTime startOfSale)
    {
        ProductCode = productCode;
        ProductName = productName;
        StartOfSale = startOfSale;
    }
    
    public string ProductCode { get; init; }
    public string ProductName { get; init; }
    public DateTime StartOfSale { get; init; }
}
