using CucumberExpressions;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings;

public class RegexStepDefinitionBindingBuilder : StepDefinitionBindingBuilderBase
{
    public RegexStepDefinitionBindingBuilder(StepDefinitionType stepDefinitionType, IBindingMethod bindingMethod, BindingScope bindingScope, string sourceExpression) : base(stepDefinitionType, bindingMethod, bindingScope, sourceExpression)
    {
    }

    protected override IExpression CreateExpression(out string expressionType)
    {
        var regexSource = GetRegexSource(out expressionType);
        var regex = RegexFactory.CreateRegexForBindings(regexSource);
        var expression = new RegularExpression(regex);
        return expression;
    }

    protected virtual string GetRegexSource(out string expressionType)
    {
        expressionType = StepDefinitionExpressionTypes.RegularExpression;
        return RegexFactory.GetWholeTextMatchRegexSource(_sourceExpression);
    }
}