using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings
{
    public interface IBindingFactory
    {
        IHookBinding CreateHookBinding(IBindingMethod bindingMethod, HookType hookType, BindingScope bindingScope,
            int hookOrder);

        IStepDefinitionBindingBuilder CreateStepDefinitionBindingBuilder(StepDefinitionType stepDefinitionType, IBindingMethod bindingMethod, BindingScope bindingScope,
            string expressionString, ExpressionType expressionType);

        IStepArgumentTransformationBinding CreateStepArgumentTransformation(string regexString,
            IBindingMethod bindingMethod, string parameterTypeName = null,
            int order = StepArgumentTransformationAttribute.DefaultOrder);
    }
}
