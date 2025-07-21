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

/// <summary>
/// This class is used at test start-up to iterate through the <see cref="IBindingRegistry"/> to generate messages for each of the 
/// <see cref="IStepArgumentTransformationBinding"/>, <see cref="IStepDefinitionBinding"/> and <see cref="IHookBinding"/>.
/// The binding items found are also cached for use during the processing of test cases.
/// </summary>
internal class BindingMessagesGenerator : IBindingMessagesGenerator
{
    private IReadOnlyDictionary<IBinding, string> _cachedBindings;
    private IEnumerable<Envelope> _cachedMessages;

    private readonly IIdGenerator _idGenerator;
    private readonly ICucumberMessageFactory _messageFactory;
    private readonly IBindingRegistry _bindingRegistry;
    private bool _initialized = false;

    private readonly HashSet<IStepArgumentTransformationBinding> _processedStepArgumentTransforms = new();
    private readonly HashSet<IStepDefinitionBinding> _processedStepDefinitionsWithUndefinedParameterType = new();

    public bool Ready => _initialized && _cachedBindings != null;
    public IReadOnlyDictionary<IBinding, string> StepDefinitionIdByBinding => PullBindings();
    public IEnumerable<Envelope> StaticBindingMessages => PullMessages();

    public BindingMessagesGenerator(IIdGenerator idGenerator, ICucumberMessageFactory messageFactory, IBindingRegistry bindingRegistry)
    {
        _idGenerator = idGenerator;
        _messageFactory = messageFactory;
        _bindingRegistry = bindingRegistry;
        _bindingRegistry.BindingRegistryReadyEvent += OnBindingRegistryReady;
    }

    internal void OnBindingRegistryReady(object sender, BindingRegistryReadyEventArgs e)
    {
        if (!_initialized && _bindingRegistry.Ready)
        {
            PopulateBindingCachesAndGenerateBindingMessages(out var messages, out var idsByBinding);
            _cachedBindings = new ReadOnlyDictionary<IBinding, string>(idsByBinding);
            _cachedMessages = messages;
            _initialized = true;
        }
    }

    private IReadOnlyDictionary<IBinding, string> PullBindings()
    {
        if (_initialized)
            return _cachedBindings;

        throw new ApplicationException("Formatters asked to provide IBindings before they were ready.");
    }

    private IEnumerable<Envelope> PullMessages()
    {
        if (_initialized)
            return _cachedMessages;

        throw new ApplicationException("Formatters asked to provide static Messages before they ready.");
    }

    private void PopulateBindingCachesAndGenerateBindingMessages(out IEnumerable<Envelope> messages, out IDictionary<IBinding, string> idsByBinding)
    {
        var resultMessages = new List<Envelope>();
        var cachedBindings = new Dictionary<IBinding, string>();

        foreach (var stepTransform in _bindingRegistry.GetStepTransformations())
        {
            if (!_processedStepArgumentTransforms.Add(stepTransform))
                continue;
            var parameterType = _messageFactory.ToParameterType(stepTransform, _idGenerator);
            resultMessages.Add(Envelope.Create(parameterType));
        }

        foreach (var binding in _bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
        {
            var errorMessage = binding.ErrorMessage;
            if (errorMessage.Contains("Undefined parameter type"))
            {
                var paramName = Regex.Match(errorMessage, "Undefined parameter type '(.*)'").Groups[1].Value;
                if (!_processedStepDefinitionsWithUndefinedParameterType.Add(binding))
                    continue;
                var undefinedParameterType = _messageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, _idGenerator);
                resultMessages.Add(Envelope.Create(undefinedParameterType));
            }
        }

        foreach (var binding in _bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
        {
            if (cachedBindings.ContainsKey(binding))
                continue;
            var stepDefinition = _messageFactory.ToStepDefinition(binding, _idGenerator);
            cachedBindings.Add(binding, stepDefinition.Id);
            resultMessages.Add(Envelope.Create(stepDefinition));
        }

        foreach (var hookBinding in _bindingRegistry.GetHooks())
        {
            if (cachedBindings.ContainsKey(hookBinding))
                continue;
            var hook = _messageFactory.ToHook(hookBinding, _idGenerator);
            cachedBindings.Add(hookBinding, hook.Id);
            resultMessages.Add(Envelope.Create(hook));
        }

        messages = resultMessages;
        idsByBinding = cachedBindings;
    }
}

