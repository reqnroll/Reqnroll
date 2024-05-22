using CucumberExpressions;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings.CucumberExpressions;

public class CucumberExpressionStepDefinitionBindingBuilder(
    CucumberExpressionParameterTypeRegistry _cucumberExpressionParameterTypeRegistry,
    StepDefinitionType stepDefinitionType,
    IBindingMethod bindingMethod,
    BindingScope bindingScope,
    string sourceExpression)
    : StepDefinitionBindingBuilderBase(stepDefinitionType, bindingMethod, bindingScope, sourceExpression)
{
    protected override IExpression CreateExpression(out string expressionType)
    {
        expressionType = StepDefinitionExpressionTypes.CucumberExpression;
        return new ReqnrollCucumberExpression(_sourceExpression, _cucumberExpressionParameterTypeRegistry);
    }
}