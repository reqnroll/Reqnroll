using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
                else if (CucumberExpressionDetector.IsCucumberExpression(definitionSyntax.TextPattern))
                {
                    bindingMethod = BindingMethod.CucumberExpression;

                    var prefix = definitionSyntax.MatchedKeywords == StepKeywordMatch.Any ?
                        "*" :
                        definitionSyntax.MatchedKeywords.ToString();

                    displayName = $"{prefix} {definitionSyntax.TextPattern}";
                }
                else
                {
                    bindingMethod = BindingMethod.RegularExpression;
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
                var groups = definitions
                    .GroupBy(static definition => definition.Method.DeclaringClassName)
                    .ToDictionary(static group => group.Key, group => group.ToImmutableArray()); 

                var nameMap = new Dictionary<QualifiedTypeName, string>();
                var result = ImmutableList.CreateBuilder<StepDefinitionGroup>();

                foreach (var group in groups)
                {
                    // If the name already exists in the map, use it.
                    if (nameMap.TryGetValue(group.Key, out var name))
                    {
                        result.Add(new StepDefinitionGroup(name, group.Value));
                        continue;
                    }

                    // If the class name is unique, use it directly.
                    if (groups.Keys.Count(key => key.Name == group.Key.Name) == 1)
                    {
                        result.Add(new StepDefinitionGroup(group.Key.Name, group.Value));
                        continue;
                    }

                    // If there are multiple classes with the same name, prefix with the namespace parts until all are unique.
                    var collisions = groups.Keys.Where(key => key.Name == group.Key.Name).ToArray();
                    var candiates = collisions.Select(static qualifiedName => qualifiedName.Name).ToArray();
                    var namespaces = collisions.Select(static qualifiedName => qualifiedName.Namespace.Split('.')).ToArray();
                    var prefixCount = 0;

                    do
                    {
                        var noMoreParts = true;

                        for (var i = 0; i < collisions.Length; i++)
                        {
                            prefixCount++;
                            var ns = namespaces[i];

                            if (ns.Length < prefixCount)
                            {
                                // Prepend the next namespace part to the candidate.
                                candiates[i] = ns[ns.Length - prefixCount] + "_" + candiates[i];
                                noMoreParts = false;
                            }
                        }

                        if (noMoreParts)
                        {
                            // This would mean there are two or more step class symbols with the same name.
                            break;
                        }
                    }
                    while (candiates.Distinct().Count() != candiates.Length);

                    // Include all generated names in the map to avoid reprocessing.
                    for (var i = 0; i < collisions.Length; i++)
                    {
                        nameMap[collisions[i]] = candiates[i];
                    }

                    // Add the group for the generated name
                    result.Add(new StepDefinitionGroup(nameMap[group.Key], group.Value));
                }

                return result;
            });

        // Output the step registry constructor which roots all defined step methods
        context.RegisterSourceOutput(stepGroups.Collect(), (context, stepGroups) =>
        {
            if (!stepGroups.Any())
            {
                return;
            }

            var emitter = new RegistryClassEmitter("Sample.Tests");

            context.AddSource("ReqnrollStepRegistry.g.cs", emitter.EmitRegistryClassConstructor(stepGroups));
        });

        // Output the step catalog classes
        context.RegisterSourceOutput(stepGroups, (context, stepGroup) =>
        {
            var emitter = new RegistryClassEmitter("Sample.Tests");

            context.AddSource(
                $"{stepGroup.GroupName}.g.cs",
                emitter.EmitStepGroupRegisterMethod(stepGroup));
        });
    }
}
