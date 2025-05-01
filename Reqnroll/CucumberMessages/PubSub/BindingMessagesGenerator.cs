using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reqnroll.CucumberMessages.PubSub
{
    /// <summary>
    /// This class is used at test start-up to iterate through the BindingRegistry to generate Messages for each of the 
    /// StepTransformations, StepDefinitions and Hooks.
    /// The binding items found are also cached for use during the processing of test cases.
    /// </summary>
    internal class BindingMessagesGenerator
    {
        public IEnumerable<Envelope> PopulateBindingCachesAndGenerateBindingMessages(IBindingRegistry bindingRegistry, 
            IIdGenerator idGenerator,
            ConcurrentBag<IStepArgumentTransformationBinding> stepArgumentTransformCache,
            ConcurrentBag<IStepDefinitionBinding> undefinedParameterTypeBindingsCache,
            ConcurrentDictionary<string, string> stepDefinitionsByPatternCache)
        {
            foreach (var stepTransform in bindingRegistry.GetStepTransformations())
            {
                if (stepArgumentTransformCache.Contains(stepTransform))
                    continue;
                stepArgumentTransformCache.Add(stepTransform);
                var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, idGenerator);
                yield return Envelope.Create(parameterType);
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
            {
                var errmsg = binding.ErrorMessage;
                if (errmsg.Contains("Undefined parameter type"))
                {
                    var paramName = Regex.Match(errmsg, "Undefined parameter type '(.*)'").Groups[1].Value;
                    if (undefinedParameterTypeBindingsCache.Contains(binding))
                        continue;
                    undefinedParameterTypeBindingsCache.Add(binding);
                    var undefinedParameterType = CucumberMessageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, idGenerator);
                    yield return Envelope.Create(undefinedParameterType);
                }
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
            {
                var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                if (stepDefinitionsByPatternCache.ContainsKey(pattern))
                    continue;
                var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, idGenerator);
                if (stepDefinitionsByPatternCache.TryAdd(pattern, stepDefinition.Id))
                {
                    yield return Envelope.Create(stepDefinition);
                }
            }

            foreach (var hookBinding in bindingRegistry.GetHooks())
            {
                var hookId = CucumberMessageFactory.CanonicalizeHookBinding(hookBinding);
                if (stepDefinitionsByPatternCache.ContainsKey(hookId))
                    continue;
                var hook = CucumberMessageFactory.ToHook(hookBinding, idGenerator);
                if (stepDefinitionsByPatternCache.TryAdd(hookId, hook.Id))
                {
                    yield return Envelope.Create(hook);
                }
                ;
            }
        }


    }
}
