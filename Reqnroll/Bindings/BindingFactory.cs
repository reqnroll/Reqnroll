using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings;

public class BindingFactory(
    IStepDefinitionRegexCalculator _stepDefinitionRegexCalculator, 
    ICucumberExpressionStepDefinitionBindingBuilderFactory _cucumberExpressionStepDefinitionBindingBuilderFactory,
    ICucumberExpressionDetector _cucumberExpressionDetector)
    : IBindingFactory
{
    public IHookBinding CreateHookBinding(IBindingMethod bindingMethod, HookType hookType, BindingScope bindingScope,
        int hookOrder)
    {
        return new HookBinding(bindingMethod, hookType, bindingScope, hookOrder);
    }

    public IStepDefinitionBindingBuilder CreateStepDefinitionBindingBuilder(StepDefinitionType stepDefinitionType, IBindingMethod bindingMethod, BindingScope bindingScope, string expressionString)
    {
        return expressionString == null
            ? new MethodNameStepDefinitionBindingBuilder(_stepDefinitionRegexCalculator, stepDefinitionType, bindingMethod, bindingScope)
            : _cucumberExpressionDetector.IsCucumberExpression(expressionString)
                ? _cucumberExpressionStepDefinitionBindingBuilderFactory.Create(stepDefinitionType, bindingMethod, bindingScope, expressionString)
                : new RegexStepDefinitionBindingBuilder(stepDefinitionType, bindingMethod, bindingScope, expressionString);
    }

    public IStepArgumentTransformationBinding CreateStepArgumentTransformation(string regexString,
        IBindingMethod bindingMethod, string parameterTypeName = null, int order = default)
    {
        return new StepArgumentTransformationBinding(regexString, bindingMethod, parameterTypeName, order);
    }
}
