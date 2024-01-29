namespace Reqnroll.MsBuildNetSdk.IntegrationTests.Features
{
    [Binding]
    public class DoNothingBinding
    {
        [Given(".*"), When(".*"), Then(".*")]
        public void EmptyStep()
        {
        }

        [Given(".*"), When(".*"), Then(".*")]
        public void EmptyStep(string multiLineStringParam)
        {
        }

        [Given(".*"), When(".*"), Then(".*")]
        public void EmptyStep(DataTable tableParam)
        {
        }
    }
}