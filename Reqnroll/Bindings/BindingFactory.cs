using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;

namespace Reqnroll.Bindings;

public class BindingFactory : IBindingFactory
{
    private readonly IStepDefinitionRegexCalculator _stepDefinitionRegexCalculator;
    private readonly ICucumberExpressionStepDefinitionBindingBuilderFactory _cucumberExpressionStepDefinitionBindingBuilderFactory;
    private readonly ReqnrollConfiguration _reqnrollConfiguration;

    public BindingFactory(
        IStepDefinitionRegexCalculator stepDefinitionRegexCalculator,
        ICucumberExpressionStepDefinitionBindingBuilderFactory cucumberExpressionStepDefinitionBindingBuilderFactory,
        ReqnrollConfiguration reqnrollConfiguration)
    {
        this._stepDefinitionRegexCalculator = stepDefinitionRegexCalculator;
        this._cucumberExpressionStepDefinitionBindingBuilderFactory = cucumberExpressionStepDefinitionBindingBuilderFactory;
        this._reqnrollConfiguration = reqnrollConfiguration;
    }

    public IHookBinding CreateHookBinding(IBindingMethod bindingMethod, HookType hookType, BindingScope bindingScope,
        int hookOrder)
    {
        return new HookBinding(bindingMethod, hookType, bindingScope, hookOrder);
    }

    public IStepDefinitionBindingBuilder CreateStepDefinitionBindingBuilder(StepDefinitionType stepDefinitionType, IBindingMethod bindingMethod, BindingScope bindingScope, string expressionString)
    {
        if (expressionString == null)
        {
            return new MethodNameStepDefinitionBindingBuilder(_stepDefinitionRegexCalculator, stepDefinitionType, bindingMethod, bindingScope);
        }

        if (_reqnrollConfiguration.EnableCucumberStepDefinitionBindings && CucumberExpressionStepDefinitionBindingBuilder.IsCucumberExpression(expressionString))
        {
            return _cucumberExpressionStepDefinitionBindingBuilderFactory.Create(stepDefinitionType, bindingMethod, bindingScope, expressionString);
        }

        return new RegexStepDefinitionBindingBuilder(stepDefinitionType, bindingMethod, bindingScope, expressionString);
    }

    public IStepArgumentTransformationBinding CreateStepArgumentTransformation(string regexString,
        IBindingMethod bindingMethod, string parameterTypeName = null)
    {
        return new StepArgumentTransformationBinding(regexString, bindingMethod, parameterTypeName);
    }
}
