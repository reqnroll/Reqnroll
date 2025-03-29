using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class CSharpStepBindingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Obtain step definitions by looking for the various attributes
        var givenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.GivenAttribute", StepKeywordMatch.Given);
        var whenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.WhenAttribute", StepKeywordMatch.When);
        var thenStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.ThenAttribute", StepKeywordMatch.Then);
        var otherStepDefinitions = context
            .GetStepDefinitionSyntaxInfoAndConfig("Reqnroll.StepDefinitionAttribute", StepKeywordMatch.Any);

        // Combine all step definitions into a single stream and create definitions for emittings
        var allStepDefinitions = givenStepDefinitions.Collect()
            .Combine(whenStepDefinitions.Collect())
            .Combine(thenStepDefinitions.Collect())
            .Combine(otherStepDefinitions.Collect())
            .SelectMany(static (definitions, _) =>
            {
                var (((givens, whens), thens), others) = definitions;

                return givens.Concat(whens).Concat(thens).Concat(others);
            })
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
                    definitionSyntax.Method.Name,
                    displayName,
                    definitionSyntax.Method,
                    definitionSyntax.MatchedKeywords,
                    bindingMethod,
                    definitionSyntax.TextPattern,
                    invocationStyle);
            })
            .Collect();

        // Group steps definitions into their respective catalogs based on the class which declares them
        var catalogs = allStepDefinitions
            .SelectMany(static (definitions, _) => definitions
                .GroupBy(definition => definition.Method.DeclaringClassName)
                .Select(group => new StepDefinitionCatalogInfo(
                    group.Key.Name + "Catalog",
                    group.Key with { Name = group.Key.Name + "Catalog" },
                    ComparableArray.CreateRange(group))));

        // Generate unique hint names for each catalog
        var catalogHintNames = catalogs
            .Select(static (catalog, _) => (catalog.ClassName))
            .Collect()
            .Select(static (catalogIds, _) =>
            {
                var names = new HashSet<string>();
                var mappedNames = ImmutableDictionary.CreateBuilder<QualifiedTypeName, string>();

                foreach (var catalogId in catalogIds)
                {
                    var (classNamespace, className) = catalogId;
                    var hintName = className;
                    var i = 1;

                    while (names.Contains(hintName))
                    {
                        hintName = className + i++;
                    }

                    names.Add(hintName);
                    mappedNames.Add(catalogId, hintName);
                }

                return mappedNames.ToImmutable();
            })
            .WithComparer(
                ImmutableDictionaryEqualityComparer<QualifiedTypeName, string>.Instance);

        // Combine the catalogs with their hint names
        var hintedCatalogs = catalogs
            .Combine(catalogHintNames)
            .Select(static (catalogAndHintNames, _) =>
            {
                var (catalog, hintNames) = catalogAndHintNames;

                if (hintNames.TryGetValue(catalog.ClassName, out var hintName))
                {
                    return catalog with { HintName = hintName };
                }

                return catalog;
            });

        // Output the step registry constructor which roots all defined step methods
        context.RegisterSourceOutput(allStepDefinitions, (context, stepMethods) =>
        {
            if (!stepMethods.Any())
            {
                return;
            }

            var emitter = new RegistryClassEmitter("Sample.Tests");

            context.AddSource("ReqnrollStepRegistry.g.cs", emitter.EmitRegistryClassConstructor(stepMethods));
        });

        // Output the step catalog classes
        context.RegisterSourceOutput(hintedCatalogs, (context, stepDefinitionClass) =>
        {
            var emitter = new StepDefinitionEmitter();

            context.AddSource(
                $"{stepDefinitionClass.HintName}.g.cs",
                emitter.EmitStepDefinitionCatalogClass(stepDefinitionClass));
        });
    }
}
