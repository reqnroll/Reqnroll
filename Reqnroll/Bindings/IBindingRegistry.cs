using System;
using System.Collections.Generic;

namespace Reqnroll.Bindings
{
    public interface IBindingRegistry
    {
        event EventHandler<BindingRegistryReadyEventArgs> BindingRegistryReadyEvent;
        bool Ready { get; set; }
        bool IsValid { get; }

        IEnumerable<IStepDefinitionBinding> GetStepDefinitions();
        IEnumerable<IHookBinding> GetHooks();
        IEnumerable<IStepDefinitionBinding> GetConsideredStepDefinitions(StepDefinitionType stepDefinitionType, string stepText = null);
        IEnumerable<IHookBinding> GetHooks(HookType bindingEvent);
        IEnumerable<IStepArgumentTransformationBinding> GetStepTransformations();
        IEnumerable<BindingError> GetErrorMessages();

        void RegisterStepDefinitionBinding(IStepDefinitionBinding stepDefinitionBinding);
        void RegisterHookBinding(IHookBinding hookBinding);
        void RegisterStepArgumentTransformationBinding(IStepArgumentTransformationBinding stepArgumentTransformationBinding);
        void RegisterGenericBindingError(BindingError error);
    }
}
