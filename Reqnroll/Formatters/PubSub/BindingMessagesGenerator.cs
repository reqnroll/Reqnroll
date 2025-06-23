using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using System;
using System.Collections.ObjectModel;

namespace Reqnroll.Formatters.PubSub;

public interface IBindingMessagesGenerator
{
    IReadOnlyDictionary<IBinding, string> StepDefinitionIdByBinding { get; }
    IEnumerable<Envelope> StaticBindingMessages { get; }
}
/// <summary>
/// This class is used at test start-up to iterate through the <see cref="IBindingRegistry"/> to generate messages for each of the 
/// <see cref="IStepArgumentTransformationBinding"/>, <see cref="IStepDefinitionBinding"/> and <see cref="IHookBinding"/>.
/// The binding items found are also cached for use during the processing of test cases.
/// </summary>
internal class BindingMessagesGenerator : IBindingMessagesGenerator
{
    internal HashSet<IStepArgumentTransformationBinding> StepArgumentTransformCache { get; } = new();
    internal HashSet<IStepDefinitionBinding> UndefinedParameterTypeBindingsCache { get; } = new();
    public IReadOnlyDictionary<IBinding, string> StepDefinitionIdByBinding => PullBindings();
    private IReadOnlyDictionary<IBinding, string> _cachedBindings;
    public IEnumerable<Envelope> StaticBindingMessages => PullMessages();
    private IEnumerable<Envelope> _cachedMessages;

    private readonly IIdGenerator idGenerator;
    private readonly ICucumberMessageFactory messageFactory;
    private readonly IBindingRegistry bindingRegistry;
    private object _lock = new();
    private bool _initialized = false;

    public BindingMessagesGenerator(IIdGenerator idGenerator, ICucumberMessageFactory messageFactory, IBindingRegistry bindingRegistry)
    {
        this.idGenerator = idGenerator;
        this.messageFactory = messageFactory;
        this.bindingRegistry = bindingRegistry;
    }

    private IReadOnlyDictionary<IBinding, string> PullBindings()
    {
        if (_initialized)
            return _cachedBindings;
        lock (_lock)
        {
            if (!_initialized)
            {
                PopulateBindingCachesAndGenerateBindingMessages(out var messages, out var idsByBinding);
                _cachedBindings = new ReadOnlyDictionary<IBinding, string>(idsByBinding);
                _cachedMessages = messages;
                _initialized = true;
            }
        }
        return _cachedBindings;
    }

    private IEnumerable<Envelope> PullMessages()
    {
        if (_initialized)
            return _cachedMessages;
        lock (_lock)
        {
            if (!_initialized)
            {
                PopulateBindingCachesAndGenerateBindingMessages(out var messages, out var idsByBinding);
                _cachedBindings = new ReadOnlyDictionary<IBinding, string>(idsByBinding);
                _cachedMessages = messages;
                _initialized = true;
            }
        }
        return _cachedMessages;
    }



    private void PopulateBindingCachesAndGenerateBindingMessages(out IEnumerable<Envelope> messages, out IDictionary<IBinding, string> idsByBinding)
    {
        var resultMessages = new List<Envelope>();
        var cachedBindings = new Dictionary<IBinding, string>();

        foreach (var stepTransform in bindingRegistry.GetStepTransformations())
        {
            if (StepArgumentTransformCache.Contains(stepTransform))
                continue;
            StepArgumentTransformCache.Add(stepTransform);
            var parameterType = messageFactory.ToParameterType(stepTransform, idGenerator);
            resultMessages.Add(Envelope.Create(parameterType));
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
                resultMessages.Add(Envelope.Create(undefinedParameterType));
            }
        }

        foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
        {
            if (cachedBindings.ContainsKey(binding))
                continue;
            var stepDefinition = messageFactory.ToStepDefinition(binding, idGenerator);
            cachedBindings.Add(binding, stepDefinition.Id);
            resultMessages.Add(Envelope.Create(stepDefinition));
        }

        foreach (var hookBinding in bindingRegistry.GetHooks())
        {
            if (cachedBindings.ContainsKey(hookBinding))
                continue;
            var hook = messageFactory.ToHook(hookBinding, idGenerator);
            cachedBindings.Add(hookBinding, hook.Id);
            resultMessages.Add(Envelope.Create(hook));
        }

        messages = resultMessages;
        idsByBinding = cachedBindings;
    }
}

