using System;

namespace Reqnroll.RuntimeTests.AssistTests.ExampleEntities;

public sealed class ProductCreatedMessage : AbstractMessage<ProductCreatedMessageContent>
{
    public ProductCreatedMessage(DateTimeOffset messageCreatedAt, string productCode, string productName, DateTime startOfSale)
    {
        MessageCreatedAt = messageCreatedAt;
        MessageContent = new ProductCreatedMessageContent(productCode, productName, startOfSale);
    }
}
