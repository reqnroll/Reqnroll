using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// This class is used at test start-up to iterate through the <see cref="IBindingRegistry"/> to generate messages for each of the 
/// <see cref="IStepArgumentTransformationBinding"/>, <see cref="IStepDefinitionBinding"/> and <see cref="IHookBinding"/>.
/// The binding items found are also cached for use during the processing of test cases.
/// </summary>
internal class BindingMessagesGenerator(IIdGenerator idGenerator, ICucumberMessageFactory messageFactory)
{
    public ConcurrentBag<IStepArgumentTransformationBinding> StepArgumentTransformCache { get; } = new();
    public ConcurrentBag<IStepDefinitionBinding> UndefinedParameterTypeBindingsCache { get; } = new();
    public ConcurrentDictionary<IBinding, string> StepDefinitionIdByBinding { get; } = new();

    public IEnumerable<Envelope> PopulateBindingCachesAndGenerateBindingMessages(IBindingRegistry bindingRegistry)
    {
        foreach (var stepTransform in bindingRegistry.GetStepTransformations())
        {
            if (StepArgumentTransformCache.Contains(stepTransform))
                continue;
            StepArgumentTransformCache.Add(stepTransform);
            var parameterType = messageFactory.ToParameterType(stepTransform, idGenerator);
            yield return Envelope.Create(parameterType);
        }

        foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
        {
            var errorMessage = binding.ErrorMessage;
            if (errorMessage.Contains("Undefined parameter type"))
            {
                var paramName = Regex.Match(errorMessage, "Undefined parameter type '(.*)'").Groups[1].Value;
                if (UndefinedParameterTypeBindingsCache.Contains(binding))
                    continue;
                UndefinedParameterTypeBindingsCache.Add(binding);
                var undefinedParameterType = messageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, idGenerator);
                yield return Envelope.Create(undefinedParameterType);
            }
        }

        foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
        {
            if (StepDefinitionIdByBinding.ContainsKey(binding))
                continue;
            var stepDefinition = messageFactory.ToStepDefinition(binding, idGenerator);
            if (StepDefinitionIdByBinding.TryAdd(binding, stepDefinition.Id))
            {
                yield return Envelope.Create(stepDefinition);
            }
        }

        foreach (var hookBinding in bindingRegistry.GetHooks())
        {
            if (StepDefinitionIdByBinding.ContainsKey(hookBinding))
                continue;
            var hook = messageFactory.ToHook(hookBinding, idGenerator);
            if (StepDefinitionIdByBinding.TryAdd(hookBinding, hook.Id))
            {
                yield return Envelope.Create(hook);
            }
        }
    }
}