using System.Globalization;
using Reqnroll.Bindings;

namespace Reqnroll.BindingSkeletons
{
    public interface IStepDefinitionSkeletonProvider
    {
        string GetBindingClassSkeleton(ProgrammingLanguage language, StepInstance[] stepInstances, string namespaceName, string className, StepDefinitionSkeletonStyle style, bool asAsync, CultureInfo bindingCulture);
        string GetStepDefinitionSkeleton(ProgrammingLanguage language, StepInstance stepInstance, StepDefinitionSkeletonStyle style, bool asAsync, CultureInfo bindingCulture);
    }
}