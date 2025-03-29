﻿namespace Reqnroll.StepBindingSourceGenerator;

internal class RegistryClassEmitter(string @namespace)
{
    public string Namespace { get; } = @namespace;

    public string ClassName { get; } = "ReqnrollStepRegistry";

    /// <summary>
    /// Emits a partial class definition that contains the constructor of the registry class. The constructor has the
    /// responsibility of aggregating all set definitions into the registry.
    /// </summary>
    /// <param name="stepDefinitions">The collection of step definitions to include in the constructor.</param>
    /// <returns></returns>
    public string EmitRegistryClassConstructor(IEnumerable<StepDefinitionInfo> stepDefinitions)
    {
        var builder = new CSharpBuilder();

        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");

        builder.AppendLine();
        builder
            .Append("[assembly:Reqnroll.Bindings.ReqnrollStepRegistryAttribute(typeof(")
            .Append(Namespace)
            .Append('.')
            .Append(ClassName)
            .AppendLine("))]");
        builder.AppendLine();

        builder.Append("namespace ").AppendLine(Namespace);
        builder.AppendBodyBlock(builder =>
        {
            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// Provides a registry of all steps defined in this assembly.");
            builder.AppendLine("/// </summary>");
            builder
                .Append("public partial class ")
                .Append(ClassName)
                .Append(" : ")
                .AppendLine("Reqnroll.Bindings.StepDefinitionRegistry");

            builder.AppendBodyBlock(builder =>
            {
                builder.AppendLine("/// <summary>");
                builder.AppendLine("/// Gets the instance of the step registry.");
                builder.AppendLine("/// </summary>");
                builder
                    .Append("public static ")
                    .Append(ClassName)
                    .Append(" Instance { get; } = new ")
                    .Append(ClassName)
                    .AppendLine("();");
                builder.AppendLine();

                builder.Append("private ").Append(ClassName).AppendLine("()");
                builder.AppendBodyBlock(builder =>
                {
                    foreach (var stepDefinition in stepDefinitions)
                    {
                        builder
                            .Append("Register(global::")
                            .Append(stepDefinition.Method.DeclaringClassName.Namespace)
                            .Append('.')
                            .Append(stepDefinition.Method.DeclaringClassName.Name)
                            .Append("Catalog.")
                            .Append(stepDefinition.Name)
                            .AppendLine("Definition);");
                    }
                });
            });
        });

        return builder.ToString();
    }
}
