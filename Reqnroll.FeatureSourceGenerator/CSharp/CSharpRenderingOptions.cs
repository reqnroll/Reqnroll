namespace Reqnroll.FeatureSourceGenerator.CSharp;

/// <summary>
/// Defines the options which control the rendering of C# code.
/// </summary>
/// <param name="EnableLineMapping">Whether to include #line directives to map generated tests to lines in source feature files.</param>
/// <param name="UseNullableReferenceTypes">Whether to use nullable reference types in generated code.</param>
public record CSharpRenderingOptions(bool EnableLineMapping = true, bool UseNullableReferenceTypes = false);
