using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Ast;

namespace Reqnroll.Parser
{
    public class ReqnrollStep : Step
    {
        public ScenarioBlock ScenarioBlock { get; private set; }
        public StepKeyword StepKeyword { get; private set; }

        public ReqnrollStep(Location location, string keyword, string text, StepArgument argument, StepKeyword stepKeyword, ScenarioBlock scenarioBlock) : base(location, keyword, text, argument)
        {
            StepKeyword = stepKeyword;
            ScenarioBlock = scenarioBlock;
        }
    }
}
