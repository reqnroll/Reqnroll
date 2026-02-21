using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    private const string GeneratedStepBindingsNamespace = "Reqnroll.Generated.StepBindings";

    private static readonly char[] InvalidHintPathChars = [.. Path.GetInvalidFileNameChars(), '.'];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the assembly name and project it into the registry type name.
        var registryName = context.CompilationProvider
            .Select(static (compilation, _) =>
            {
                var assemblyName = compilation.AssemblyName ?? "Anonymous";

                const string prefix = "StepRegistry_";

                // Replace invalid chars with underscores
                var sb = new StringBuilder(prefix, assemblyName.Length + prefix.Length);
                foreach (var ch in assemblyName)
                {
                    sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');
                }

                return sb.ToString();
            });

        // Obtain step definitions by looking for the various attributes
        var givenStepDefinitions = context
            .GetStepDefinitionSyntaxInfo("Reqnroll.GivenAttribute", StepKeywordMatch.Given);
        var whenStepDefinitions = context
            .GetStepDefinitionSyntaxInfo("Reqnroll.WhenAttribute", StepKeywordMatch.When);
        var thenStepDefinitions = context
            .GetStepDefinitionSyntaxInfo("Reqnroll.ThenAttribute", StepKeywordMatch.Then);
        var otherStepDefinitions = context
            .GetStepDefinitionSyntaxInfo("Reqnroll.StepDefinitionAttribute", StepKeywordMatch.Any);

        // Combine all step definitions into a single stream and create definitions for emittings
        var allStepDefinitions = givenStepDefinitions
            .Concat(whenStepDefinitions)
            .Concat(thenStepDefinitions)
            .Concat(otherStepDefinitions)
            .Select(static (definitionSyntax, cancellationToken) =>
            {
                var displayName = definitionSyntax.Method.Name;
                BindingMethod bindingMethod;
                if (definitionSyntax.TextPattern == null)
                {
                    bindingMethod = BindingMethod.MethodName;
                }
                else
                {
                    var prefix = definitionSyntax.MatchedKeywords == StepKeywordMatch.Any ?
                        "*" :
                        definitionSyntax.MatchedKeywords.ToString();

                    displayName = $"{prefix} {definitionSyntax.TextPattern}";

                    if (CucumberExpressionDetector.IsCucumberExpression(definitionSyntax.TextPattern))
                    {
                        bindingMethod = BindingMethod.CucumberExpression;
                    }
                    else
                    {
                        bindingMethod = BindingMethod.RegularExpression;
                    }
                }

                var invocationStyle = definitionSyntax.Method.IsAsync ?
                    MethodInvocationStyle.Asynchronous :
                    MethodInvocationStyle.Synchronous;

                return new StepDefinitionInfo(
                    displayName,
                    definitionSyntax.Method,
                    definitionSyntax.MatchedKeywords,
                    bindingMethod,
                    definitionSyntax.TextPattern,
                    invocationStyle);
            });

        // Group steps definitions into their respective groups based on the class which declares them
        var stepGroups = allStepDefinitions
            .Collect()
            .SelectMany(static (definitions, _) =>
            {
                // Group step definitions by their declaring class
                return definitions
                    .GroupBy(static definition => definition.Method.DeclaringClassName)
                    .ToDictionary(static group => group.Key, group => group.ToImmutableArray())
                    .Select(static group => new StepDefinitionGroup(group.Key, group.Value));
            });

        // Output the step registry constructor which roots all defined step methods
        var registryClassInputs = registryName.Combine(stepGroups.Collect());

        context.RegisterSourceOutput(registryClassInputs, (context, inputs) =>
        {
            var (registryName, stepGroups) = inputs;

            if (!stepGroups.Any())
            {
                return;
            }

            var emitter = new RegistryClassEmitter(GeneratedStepBindingsNamespace, registryName);

            context.AddSource("ReqnrollStepRegistry.g.cs", emitter.EmitRegistryClassConstructor(stepGroups));
        });

        // Output the step catalog classes
        var stepGroupInputs = stepGroups.Combine(registryName);

        context.RegisterSourceOutput(stepGroupInputs, (context, inputs) =>
        {
            var (stepGroup, registryName) = inputs;

            var emitter = new RegistryClassEmitter(GeneratedStepBindingsNamespace, registryName);

            // Produce a unique hint name from the qualified class name.
            var hintName = new StringBuilder();

            foreach (var c in stepGroup.DeclaringClassName.ToString())
            {
                hintName.Append(InvalidHintPathChars.Contains(c) ? '_' : c);
            }

            hintName.Append(".g.cs");

            context.AddSource(
                hintName.ToString(),
                emitter.EmitStepGroupRegisterMethod(stepGroup));
        });
    }
}
