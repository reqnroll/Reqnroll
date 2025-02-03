using CucumberExpressions;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings.CucumberExpressions;

public interface IReqnrollCucumberExpressionParameterType : IParameterType
{
    IBindingType TargetType { get; }
    ICucumberExpressionParameterTypeTransformation[] Transformations { get; }
}