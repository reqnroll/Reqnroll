using System.Text;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public class ComplexProjectContentGenerator
        : IProjectContentGenerator
    {
        private static readonly string[] StepType = { "Given", "When", "Then" };
        private readonly GenerateSolutionParams generateSolutionParams;

        public ComplexProjectContentGenerator(GenerateSolutionParams generateSolutionParams)
        {
            this.generateSolutionParams = generateSolutionParams;
        }

        public void Generate(ProjectBuilder pb)
        {
            for (var i = 0; i < generateSolutionParams.FeatureCount; i++)
            {
                pb.AddFeatureFile(GenerateFeatureFileContent(i));
            }

            pb.AddStepBinding(
                @"
    [Given(""I do this"")]
    [When(""I do this"")]
    [Then(""I do this"")]
    public void Do()
    {
        System.DateTime.Now.ToString();
    }
");
        }

        public string GenerateFeatureFileContent(int featureIdx)
        {
            var text = new StringBuilder();
            text.AppendLine($"Feature: Feature #${featureIdx}");
            text.AppendLine($"Text description of feature #{featureIdx}");
            text.AppendLine();
            text.AppendLine();

            for (var scenarioIdx = 0; scenarioIdx < 10; scenarioIdx++)
            {
                text.AppendLine();
                text.AppendLine($"  Scenario: Scenario #{featureIdx}_{scenarioIdx}");
                text.AppendLine($"  Text description of scenario #{featureIdx}_{scenarioIdx}");
                text.AppendLine();

                foreach (var gvt in StepType)
                {
                    for (var stepIdx = 0; stepIdx < 4; stepIdx++)
                    {
                        text.AppendLine($"   {gvt} I do this");
                    }
                }
            }

            return text.ToString();
        }
    }
}
