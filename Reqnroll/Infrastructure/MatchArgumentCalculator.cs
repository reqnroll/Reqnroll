using Reqnroll.Bindings;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.Infrastructure
{
    // The MatchArgument structure holds information about arguments extracted from a Step match. The StartOffset indicates where in the step text the value of the argument begins in that string.
    public class MatchArgument
    {
        public object Value { get; private set; }
        public int? StartOffset { get; private set; }

        public MatchArgument(object value, int? startOffset = null)
        {
            Value = value;
            StartOffset = startOffset;
        }
    }
    public interface IMatchArgumentCalculator
    {
        MatchArgument[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding);
    }

    public class MatchArgumentCalculator : IMatchArgumentCalculator
    {
        public MatchArgument[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding)
        {
            var regexArgs = match.Groups.Cast<Group>().Skip(1).Where(g => g.Success).Select(g => new MatchArgument(g.Value, g.Index));
            var arguments = regexArgs.ToList();
            if (stepInstance.MultilineTextArgument != null)
                arguments.Add(new MatchArgument(stepInstance.MultilineTextArgument));
            if (stepInstance.TableArgument != null)
                arguments.Add(new MatchArgument(stepInstance.TableArgument));

            return arguments.ToArray();
        }
    }
}
