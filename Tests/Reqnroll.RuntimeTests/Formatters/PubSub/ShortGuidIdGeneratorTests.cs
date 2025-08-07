using Xunit;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.RuntimeTests.Formatters.PubSub;

public class ShortGuidIdGeneratorTests
{
    [Fact]
    public void GetNewId_ReturnsNonEmptyString()
    {
        using var generator = new ShortGuidIdGenerator();
        var id = generator.GetNewId();
        Assert.False(string.IsNullOrWhiteSpace(id));
    }

    [Fact]
    public void GetNewId_ReturnsUniqueIds()
    {
        using var generator = new ShortGuidIdGenerator();
        var id1 = generator.GetNewId();
        var id2 = generator.GetNewId();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void GetNewId_ReturnsBase64UrlSafeString()
    {
        using var generator = new ShortGuidIdGenerator();
        var id = generator.GetNewId();
        Assert.DoesNotContain('+', id);
        Assert.DoesNotContain('/', id);
        Assert.DoesNotContain('=', id);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimesSafely()
    {
        var generator = new ShortGuidIdGenerator();
        generator.Dispose();
        generator.Dispose(); // Should not throw
    }
}