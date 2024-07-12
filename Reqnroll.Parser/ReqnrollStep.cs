using Gherkin;
using Gherkin.Ast;

namespace Reqnroll.Parser
{
    public class ReqnrollStep : Step
    {
        public ScenarioBlock ScenarioBlock { get; private set; }
        public StepKeyword StepKeyword { get; private set; }

        public ReqnrollStep(Location location, string keyword, StepKeywordType keywordType, string text, StepArgument argument, StepKeyword stepKeyword, ScenarioBlock scenarioBlock) : base(location, keyword, keywordType, text, argument)
        {
            StepKeyword = stepKeyword;
            ScenarioBlock = scenarioBlock;
        }
    }
}
