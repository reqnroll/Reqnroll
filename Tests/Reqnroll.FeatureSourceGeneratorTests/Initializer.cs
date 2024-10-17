using System.Runtime.CompilerServices;

namespace Reqnroll.FeatureSourceGenerator;
internal static class Initializer
{
    [ModuleInitializer]
    public static void SetDefaults()
    {
        FluentAssertions.Formatting.Formatter.AddFormatter(new AttributeDescriptorFormatter());
        FluentAssertions.Formatting.Formatter.AddFormatter(new ParameterSyntaxFormatter());
    }
}
