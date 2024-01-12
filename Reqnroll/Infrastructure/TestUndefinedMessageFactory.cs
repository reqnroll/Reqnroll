using System;
using System.Globalization;
using Reqnroll.BindingSkeletons;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;

namespace Reqnroll.Infrastructure
{
    public class TestUndefinedMessageFactory : ITestUndefinedMessageFactory
    {
        private readonly IStepDefinitionSkeletonProvider _stepDefinitionSkeletonProvider;
        private readonly IErrorProvider _errorProvider;
        private readonly ReqnrollConfiguration _reqnrollConfiguration;

        public TestUndefinedMessageFactory(IStepDefinitionSkeletonProvider stepDefinitionSkeletonProvider, IErrorProvider errorProvider, ReqnrollConfiguration reqnrollConfiguration)
        {
            _stepDefinitionSkeletonProvider = stepDefinitionSkeletonProvider;
            _errorProvider = errorProvider;
            _reqnrollConfiguration = reqnrollConfiguration;
        }

        public string BuildFromContext(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            string skeleton = _stepDefinitionSkeletonProvider.GetBindingClassSkeleton(
                featureContext.FeatureInfo.GenerationTargetLanguage,
                scenarioContext.MissingSteps.ToArray(),
                "MyNamespace",
                "StepDefinitions",
                _reqnrollConfiguration.StepDefinitionSkeletonStyle,
                featureContext.BindingCulture ?? CultureInfo.CurrentCulture);

            return $"{_errorProvider.GetMissingStepDefinitionError().Message}{Environment.NewLine}{skeleton}";
        }
    }
}
