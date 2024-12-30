using Reqnroll.Assist.Dynamic;

namespace AssistDynamic.Specs.Steps
{
  [Binding]
  public class StepArgumentTransformationSteps
  {
    private readonly State state;

    public StepArgumentTransformationSteps(State state) => this.state = state;

    [Given(@"I create a set of dynamic instances from this table using step argument transformation")]
    public void a(IList<dynamic> dynamicSet)
    {
      this.state.OriginalSet = dynamicSet;
    }

    [When(@"I compare the set to this table using step argument transformation")]
    public void b(Table table)
    {
      table.CompareToDynamicSet(this.state.OriginalSet);
    }

    [Given(@"I create a dynamic instance from this table using step argument transformation")]
    public void c(dynamic instance)
    {
      this.state.OriginalInstance = instance;
    }

    [When(@"I compare it to this table using step argument transformation")]
    public void d(Table table)
    {
      var org = (object)this.state.OriginalInstance;
      table.CompareToDynamicInstance(org);
    }
  }
}
