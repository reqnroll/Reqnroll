using System.CodeDom;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestProvider;

namespace Reqnroll.Specs.Generator.ReqnrollPlugin
{
    class CustomXUnitGeneratorProvider : XUnit2TestGeneratorProvider
    {
        private readonly Combination _combination;

        public CustomXUnitGeneratorProvider(CodeDomHelper codeDomHelper, Combination combination, ProjectSettings projectSettings) : base(codeDomHelper)
        {
            _combination = combination;
        }

        public override void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            base.FinalizeTestClass(generationContext);

            if (_combination != null)
            {
                string programminLanguageEnum = $"Reqnroll.TestProjectGenerator.ProgrammingLanguage.{_combination.ProgrammingLanguage}";
                string projectFormatEnum = $"Reqnroll.TestProjectGenerator.Data.ProjectFormat.{_combination.ProjectFormat}";
                string targetFrameworkEnum = $"Reqnroll.TestProjectGenerator.Data.TargetFramework.{_combination.TargetFramework}";
                string unitTestProviderEnum = $"Reqnroll.TestProjectGenerator.UnitTestProvider.{_combination.UnitTestProvider}";
                string configFormat = $"Reqnroll.TestProjectGenerator.ConfigurationFormat.{_combination.ConfigFormat}";

                generationContext.ScenarioInitializeMethod.Statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodePropertyReferenceExpression(
                                new CodePropertyReferenceExpression(
                                    new CodeFieldReferenceExpression(null, generationContext.TestRunnerField.Name),
                                    "ScenarioContext"),
                                "ScenarioContainer"),
                            "RegisterInstanceAs",
                            new CodeTypeReference("Reqnroll.TestProjectGenerator.TestRunConfiguration")),
                        new CodeVariableReferenceExpression(
                            $"new Reqnroll.TestProjectGenerator.TestRunConfiguration(){{ ProgrammingLanguage = {programminLanguageEnum}, ProjectFormat = {projectFormatEnum}, TargetFramework = {targetFrameworkEnum}, UnitTestProvider = {unitTestProviderEnum}, ConfigurationFormat = {configFormat} }}")));
            }
        }
    }
}