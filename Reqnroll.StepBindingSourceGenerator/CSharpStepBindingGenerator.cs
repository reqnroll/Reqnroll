using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    private static readonly Regex LeadingWhitespacePattern = new(@"^\s+", RegexOptions.Compiled);
    private static readonly Regex TrailingWhitespacePattern = new(@"\s+$", RegexOptions.Compiled);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stepDefinitions = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Reqnroll.WhenAttribute",
                static (syntaxNode, _) => syntaxNode is MethodDeclarationSyntax,
                static (context, cancellationToken) => ExtractStepDeclaration(context, StepKeyword.When, cancellationToken))
            .Combine(context.AnalyzerConfigOptionsProvider)
            .SelectMany(static (stepDefinitionsAndOptionsProvider, cancellationToken) =>
            {
                var (stepDefinitions, optionsProvider) = stepDefinitionsAndOptionsProvider;

                return CombineStepDefinitionDataWithConfig(stepDefinitions, optionsProvider, cancellationToken);
            })
            .Select(static (declaration, cancellationToken) =>
            {
                var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
                var bindingMethod = BindingMethod.CucumberExpression;
                var invocationStyle = MethodInvocationStyle.Synchronous;
                string? textPattern = null;
                var name = declaration.Method.Name;
                var displayName = name;

                StepDefinitionInfo Definition()
                {
                    return new StepDefinitionInfo(
                        name,
                        displayName,
                        declaration.Method,
                        declaration.Keyword,
                        bindingMethod,
                        textPattern,
                        invocationStyle,
                        diagnostics.ToImmutable());
                }

                // Determine whether the method is intended be invoked synchronously or asynchronously.
                if (declaration.Method.ReturnsVoid)
                {
                    if (declaration.Method.IsAsync)
                    {
                        diagnostics.Add(
                            new DiagnosticInfo(
                                DiagnosticDescriptors.ErrorAsyncStepMethodMustReturnTask,
                                declaration.Method.IdentifierLocation));
                    }
                }
                else if (declaration.Method.ReturnsTask)
                {
                    invocationStyle = MethodInvocationStyle.Asynchronous;
                }
                else
                {
                    diagnostics.Add(
                        new DiagnosticInfo(
                            DiagnosticDescriptors.ErrorStepMethodMustReturnVoidOrTask,
                            declaration.Method.IdentifierLocation));
                }

                // If no step text has been specified, the step has been defined to use method-name binding.
                if (declaration.TextPattern == null)
                {
                    return Definition();
                }

                // Empty step text is not allowed (null, empty string or whitespace).
                if (string.IsNullOrWhiteSpace(declaration.TextPattern.Text))
                {
                    diagnostics.Add(
                        new DiagnosticInfo(
                            DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                            declaration.TextPattern.Location));

                    return Definition();
                }

                textPattern = declaration.TextPattern.Text!;

                // Determine whether to bind the non-empty text as a Cucumber expression or a regular expression.
                // The default for Reqnroll is Cucumber expression.
                var bindingStyle = CucumberExpressionDetector.IsCucumberExpression(textPattern) ?
                    BindingMethod.CucumberExpression :
                    BindingMethod.RegularExpression;

                CheckStepTextForLeadingWhitespace(textPattern, declaration.TextPattern.Location, diagnostics);
                CheckStepTextForTrailingWhitespace(textPattern, declaration.TextPattern.Location, diagnostics);

                return Definition();
            })
            .Where(stepDefinition => stepDefinition is not null)!;

        var eligableStepDefinitions = stepDefinitions.Where(static method => method.IsEligibleForRegistry).Collect();

        // Output the step registry constructor which roots all defined step methods.
        context.RegisterSourceOutput(eligableStepDefinitions, (context, stepMethods) =>
        {
            if (!stepMethods.Any())
            {
                return;
            }

            var emitter = new RegistryClassEmitter("Sample.Tests");

            context.AddSource("ReqnrollStepRegistry.g.cs", emitter.EmitRegistryClassConstructor(stepMethods));
        });

        // Output the definition class for every step definition type.
        var stepDefinitionClasses = eligableStepDefinitions
            .SelectMany((stepDefinitions, _) => stepDefinitions
                .GroupBy(step => (step.Method.DeclaringClassNamespace, step.Method.DeclaringClassName))
                .Select(group => new StepDefinitionClassInfo(
                    group.Key.DeclaringClassNamespace,
                    group.Key.DeclaringClassName,
                    group.ToImmutableArray())));

        context.RegisterSourceOutput(stepDefinitionClasses, (context, stepDefinitionClass) =>
        {
            var emitter = new StepDefinitionEmitter();

            context.AddSource(
                $"{stepDefinitionClass.ClassNamespace}/{stepDefinitionClass.ClassName}.g.cs",
                emitter.EmitStepDefinitionClass(stepDefinitionClass));
        });

        // Report any diagnostics encountered during generation.
        context.RegisterSourceOutput(
            stepDefinitions.Where(static stepDefinition => stepDefinition.HasDiagnostics),
            static (context, stepDefinition) =>
            {
                foreach (var diagnostic in stepDefinition.Diagnostics)
                {
                    context.ReportDiagnostic(diagnostic.CreateDiagnostic());
                }
            });
    }

    /// <summary>
    /// Processes a set of step definition data to produce <see cref="StepDefinitionInfo"/> instances.
    /// This method is likely to be called for every change to affected attributes due to 
    /// <see cref="StepDeclarationInfo"/> containing a reference to a syntax node which prevents it from being an
    /// effective key value.
    /// </summary>
    /// <param name="data">The source collection to use as the basis for creating step definitions. The collection
    /// may be empty or not initialized.</param>
    /// <param name="optionsProvider">The options proivder to use to obtain options that influence the generation process.</param>
    /// <param name="cancellationToken">A token used to signal when the operation should be canceled.</param>
    /// <returns>A collection of <see cref="StepDeclarationInfoAndConfig"/> that represent the combined step declaration
    /// and configuration.</returns>
    private static IEnumerable<StepDeclarationInfoAndConfig> CombineStepDefinitionDataWithConfig(
        ComparableArray<StepDeclarationInfo> data,
        AnalyzerConfigOptionsProvider optionsProvider,
        CancellationToken cancellationToken)
    {
        if (data.IsDefaultOrEmpty)
        {
            yield break;
        }
        
        foreach (var stepDefinitionData in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = optionsProvider.GetOptions(stepDefinitionData.MethodDeclarationSyntax.SyntaxTree);

            yield return CombineStepDefinitionDataWithConfig(stepDefinitionData, options, cancellationToken);
        }
    }

    /// <summary>
    /// Processes a <see cref="StepDeclarationInfo"/> with <see cref="AnalyzerConfigOptions"/> to produce a
    /// <see cref="StepDeclarationInfoAndConfig"/> instance which either contains all relevent data of a step definition
    /// to be processed, plus all configuration which affects the processing of the step.
    /// </summary>
    /// <param name="stepDefinitionData">The step definition information to process.</param>
    /// <param name="options">Options which apply to this step definition.</param>
    /// <param name="cancellationToken">A token used to signal when the process should be canceled.</param>
    /// <returns>A <see cref="StepDeclarationInfoAndConfig"/> representing processed step definition.</returns>
    private static StepDeclarationInfoAndConfig CombineStepDefinitionDataWithConfig(
        StepDeclarationInfo stepDefinitionData,
        AnalyzerConfigOptions options,
        CancellationToken cancellationToken)
    {
        
    }

    /// <summary>
    /// This method extracts the essential data for each attribute. The objective is to do as little work as possible as
    /// this method is going to be called every time a Given, When, Then or StepDefinition attribute is modified
    /// or the method they're attached to is modified.
    /// </summary>
    /// <param name="context">The context containing the syntax information for matched attributes.</param>
    /// <param name="keyword">The keyword associated with the matched attributes.</param>
    /// <param name="cancellationToken">A token used to signal when the process should be cancelled.</param>
    /// <returns>An array of the step definition data extracted from the matched attributes.</returns>
    private static ComparableArray<StepDeclarationInfo> ExtractStepDeclaration(
        GeneratorAttributeSyntaxContext context,
        StepKeyword keyword,
        CancellationToken cancellationToken)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var methodSymbol = (IMethodSymbol?)context.SemanticModel.GetDeclaredSymbol(methodSyntax);

        // If the semantic model does not contain the method, we can't bind to it.
        // This usually indicates a problem with the general syntax of this method.
        if (methodSymbol == null)
        {
            return ComparableArray<StepDeclarationInfo>.Empty;
        }

        var result = ImmutableArray.CreateBuilder<StepDeclarationInfo>();

        foreach (var attribute in context.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the attribute candidate is an error symbol, skip it.
            if (attribute.AttributeClass == null || attribute.AttributeClass.Kind == SymbolKind.ErrorType)
            {
                continue;
            }

            attribute.ConstructorArguments.

            var data = new StepDeclarationInfo(methodSyntax);

            result.Add(data);
        }

        return result.ToImmutable();
    }

    private static StepMethodInfo? GetStepMethodInfo(
        GeneratorAttributeSyntaxContext context,
        StepKeyword keyword,
        CancellationToken cancellationToken)
    {
        var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
        var methodSymbol = (IMethodSymbol?)context.SemanticModel.GetDeclaredSymbol(methodSyntax);

        // If the semantic model does not contain the method, we can't bind to it.
        // This usually indicates a problem with the general syntax of this method.
        if (methodSymbol == null)
        {
            return null;
        }

        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
        var bindings = ImmutableArray.CreateBuilder<StepBindingInfo>();
        var invocationStyle = MethodInvocationStyle.Synchronous;

        // Determine whether the method is intended be invoked synchronously or asynchronously.
        if (methodSymbol.ReturnsVoid)
        {
            if (methodSymbol.IsAsync)
            {
                diagnostics.Add(
                    new DiagnosticInfo(
                        DiagnosticDescriptors.ErrorAsyncStepMethodMustReturnTask,
                        DiagnosticLocation.CreateFrom(methodSyntax.Identifier)));
            }
        }
        else if (methodSymbol.ReturnType.ToDisplayString() == "System.Threading.Tasks.Task")
        {
            invocationStyle = MethodInvocationStyle.Asynchronous;
        }
        else
        {
            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.ErrorStepMethodMustReturnVoidOrTask,
                    DiagnosticLocation.CreateFrom(methodSyntax.Identifier)));
        }

        foreach (var attribute in context.Attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bindingInfo = GetStepBindingInfo(attribute, keyword);

            if (bindingInfo != null)
            {
                bindings.Add(bindingInfo);
            }
        }

        return new StepMethodInfo(
            methodSymbol.Name,
            methodSymbol.ContainingType.Name,
            methodSymbol.ContainingType.ContainingNamespace.Name,
            invocationStyle,
            new ComparableArray<StepBindingInfo>(bindings.ToImmutable()),
            new ComparableArray<DiagnosticInfo>(diagnostics.ToImmutable()));
    }

    private static StepBindingInfo? GetStepBindingInfo(
        AttributeData attribute,
        StepKeyword keyword)
    {
        // If the attribute candidate is an error symbol, skip it.
        if (attribute.AttributeClass == null || attribute.AttributeClass.Kind == SymbolKind.ErrorType)
        {
            return null;
        }

        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();

        StepBindingInfo CreateBindingInfo(BindingMethod style, string? text)
        {
            return new StepBindingInfo(
                style,
                keyword,
                text,
                new ComparableArray<DiagnosticInfo>(diagnostics.ToImmutable()));
        }

        // If the attribute candidate does not have any arguments, we're matching based on the method name.
        if (attribute.ConstructorArguments.Length == 0)
        {
            return CreateBindingInfo(BindingMethod.MethodName, null);
        }

        // If the attribute has any arguments, the first is the step text.
        var textConstant = attribute.ConstructorArguments[0];
        var text = (string?)textConstant.Value;

        // Any further diagnostics will want to refer to the step text location with precision.
        // Unfortunately Roslyn generator doesn't give us the syntax nodes for the attribute arguments,
        // so we have to examine the attribute syntax to find the matching literal node.
        var textExpressionSyntax = GetStepTextExpressionSyntax(attribute);
        var textLocation = DiagnosticLocation.CreateFrom(
            textExpressionSyntax ?? attribute.ApplicationSyntaxReference?.GetSyntax());

        // Empty step text is not allowed (null, empty string or whitespace).
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    textLocation));

            // There's not really any text to bind to, but we still need to return a binding style
            // and it's definitely not "Method Name", so we'll use the Reqnroll default for text.
            return CreateBindingInfo(BindingMethod.CucumberExpression, text);
        }

        // Determine whether to bind the non-empty text as a Cucumber expression or a regular expression.
        // The default for Reqnroll is Cucumber expression.
        var style = CucumberExpressionDetector.IsCucumberExpression(text!) ?
            BindingMethod.CucumberExpression :
            BindingMethod.RegularExpression;

        CheckStepTextForLeadingWhitespace(text!, textExpressionSyntax, textLocation, diagnostics);
        CheckStepTextForTrailingWhitespace(text!, textExpressionSyntax, textLocation, diagnostics);

        return CreateBindingInfo(style, text);
    }

    private static void CheckStepTextForTrailingWhitespace(
        string text,
        DiagnosticLocation textLocation,
        ImmutableArray<DiagnosticInfo>.Builder diagnostics)
    {
        var trailingWhitespaceMatch = TrailingWhitespacePattern.Match(text);
        if (trailingWhitespaceMatch.Success)
        {
            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.WarningStepTextHasTrailingWhitespace,
                    textLocation));
        }
    }

    private static void CheckStepTextForLeadingWhitespace(
        string text,
        DiagnosticLocation textLocation,
        ImmutableArray<DiagnosticInfo>.Builder diagnostics)
    {
        var leadingWhitespaceMatch = LeadingWhitespacePattern.Match(text);
        if (leadingWhitespaceMatch.Success)
        {
            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.WarningStepTextHasLeadingWhitespace,
                    textLocation));
        }
    }

    private static ExpressionSyntax? GetStepTextExpressionSyntax(AttributeData attribute)
    {
        var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;

        if (attributeSyntax?.ArgumentList != null)
        {
            foreach (var argument in attributeSyntax.ArgumentList.Arguments)
            {
                if (argument.NameEquals != null)
                {
                    continue;
                }

                if (argument.NameColon == null)
                {
                    return argument.Expression;
                }

                if (argument.NameColon.Name.Identifier.Text == "regex")
                {
                    return argument.Expression;
                }
            }
        }

        return null;
    }
}

/// <summary>
/// Represents the pipline-relevant information about the declaration of a step definition.
/// </summary>
/// <param name="MethodDeclarationSyntax">The method declaration syntax which has been identified to be a step 
/// definition.</param>
internal record StepDeclarationInfo(
    MethodDeclarationSyntax MethodDeclarationSyntax);

/// <summary>
/// Represents the combined step declaration data and the config which applies to it.
/// </summary>
internal record StepDeclarationInfoAndConfig(
    MethodInfo Method,
    StepTextPatternInfo? TextPattern,
    StepKeyword Keyword,
    bool IsSourceGenerationEnabled);

internal record StepTextPatternInfo(string? Text, DiagnosticLocation Location);

internal record MethodInfo(
    string Name,
    string DeclaringClassName,
    string DeclaringClassNamespace,
    DiagnosticLocation IdentifierLocation,
    bool ReturnsVoid,
    bool ReturnsTask,
    bool IsAsync,
    ComparableArray<ParameterInfo> Parameters);

internal record ParameterInfo(string Name, string ParameterType);

internal enum StepKeyword
{
    Given,
    When,
    Then
}

internal enum MethodInvocationStyle
{
    Synchronous,
    Asynchronous
}

internal record StepDefinitionInfo(
    string Name,
    string DisplayName,
    MethodInfo Method,
    StepKeyword Keyword,
    BindingMethod BindingMethod,
    string? TextPattern,
    MethodInvocationStyle InvocationStyle,
    ComparableArray<DiagnosticInfo> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    public bool IsEligibleForRegistry => !HasErrors;

    public bool HasDiagnostics => Diagnostics.Length > 0;
}

internal record StepDefinitionClassInfo(
    string ClassName,
    string ClassNamespace,
    ComparableArray<StepDefinitionInfo> StepDefinitions);

internal static class ImmutableCollection
{
    public static ComparableArray<T> Create<T>(T item) => new ComparableArray<T>(ImmutableArray.Create(item));

    public static ComparableArray<T> Create<T>(ReadOnlySpan<T> values)
    {
        var builder = ImmutableArray.CreateBuilder<T>(values.Length);

        foreach (var item in values)
        {
            builder.Add(item);
        }

        return new ComparableArray<T>(builder.ToImmutable());
    }
}

internal readonly struct ComparableArray<T>(ImmutableArray<T> values) : IEquatable<ComparableArray<T>>, IEnumerable<T>
{
    private readonly ImmutableArray<T> _values = values;

    public static ComparableArray<T> Empty { get; } = new ComparableArray<T>(ImmutableArray<T>.Empty);

    public int Length => _values.Length;

    public bool IsDefaultOrEmpty => _values.IsDefaultOrEmpty;

    public override bool Equals(object obj)
    {
        if (obj is ComparableArray<T> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(ComparableArray<T> other)
    {
        if (_values.IsDefaultOrEmpty)
        {
            return other._values.IsDefaultOrEmpty;
        }

        if (other._values.IsDefaultOrEmpty)
        {
            return false;
        }

        // This comparison checks if the two immutable arrays point to the same underlying array object.
        if (_values == other._values)
        {
            return true;
        }

        if (_values.Length != other._values.Length)
        {
            return false;
        }

        for (var i = 0; i < _values.Length; i++)
        {
            if (!Equals(_values[i], other._values[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        if (_values.IsDefaultOrEmpty)
        {
            return 0;
        }

        return _values.GetSequenceHashCode();
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => 
        (_values.IsDefault ? ImmutableArray<T>.Empty : _values).GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => 
        ((IEnumerable<T>)(_values.IsDefault ? ImmutableArray<T>.Empty : _values)).GetEnumerator();

    public static implicit operator ComparableArray<T>(ImmutableArray<T> array) => new(array);
}

internal enum BindingMethod
{
    RegularExpression,
    CucumberExpression,
    MethodName
}

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    DiagnosticLocation Location,
    params object?[]? MessageArguments)
{
    public DiagnosticSeverity Severity => Descriptor.DefaultSeverity;

    public Diagnostic CreateDiagnostic() => Diagnostic.Create(Descriptor, Location, MessageArguments);

    public bool Equals(DiagnosticInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!(Descriptor.Equals(other.Descriptor) &&
            Location.Equals(other.Location)))
        {
            return false;
        }
        
        if (MessageArguments == null)
        {
            return other.MessageArguments == null;
        }

        if (other.MessageArguments == null)
        {
            return false;
        }

        return MessageArguments.SequenceEqual(other.MessageArguments);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 4037797;

            hash *= 1864987 + Descriptor.GetHashCode();
            hash *= 1864987 + Location.GetHashCode();
            hash *= 1864987 + MessageArguments?.GetSequenceHashCode() ?? 0;

            return hash;
        }
    }
}

internal record struct DiagnosticLocation(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public static DiagnosticLocation None { get; } = default;

    public readonly Location? ToLocation() => FilePath == null ? null : Location.Create(FilePath, TextSpan, LineSpan);

    public static DiagnosticLocation CreateFrom(SyntaxNode? node) => node == null ? None : CreateFrom(node.GetLocation());

    public static DiagnosticLocation CreateFrom(Location location)
    {
        if (location.SourceTree == null)
        {
            throw new ArgumentException("Diagnostic location must be from a source location.", nameof(location));
        }

        return new DiagnosticLocation(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public static DiagnosticLocation CreateFrom(SyntaxReference? syntaxReference)
    { 
        if (syntaxReference == null)
        {
            return None;
        }

        return CreateFrom(syntaxReference.SyntaxTree.GetLocation(syntaxReference.Span));
    }

    public static DiagnosticLocation CreateFrom(SyntaxToken token) => CreateFrom(token.GetLocation());

    public static implicit operator Location?(DiagnosticLocation location) => location.ToLocation();
}
