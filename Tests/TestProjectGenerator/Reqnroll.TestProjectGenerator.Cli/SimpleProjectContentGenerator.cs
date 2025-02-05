namespace Reqnroll.TestProjectGenerator.Cli
{
    public class SimpleProjectContentGenerator
        : IProjectContentGenerator
    {
        public void Generate(ProjectBuilder pb)
        {
            pb.AddFeatureFile(@"
Feature: Simple Feature
	Scenario: Simple Scenario
		Given I use a .NET API
");

            pb.AddStepBinding(@"
    [Given(""I use a .NET API"")]
    public void Do()
    {
        System.DateTime.Now.ToString();
    }
");
        }
    }
}
