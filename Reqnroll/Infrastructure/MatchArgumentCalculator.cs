using Reqnroll.Bindings;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.Infrastructure
{
    // The ArgumentInfo structure holds information about arguments extracted from a Step match. The StartOffset indicates where in the step text the value of the argument begins in that string.
    public class ArgumentInfo
    {
        public object Value;
        public int StartOffset;
    }
    public interface IMatchArgumentCalculator
    {
        object[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding);
    }

    public class MatchArgumentCalculator : IMatchArgumentCalculator
    {
        public object[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding)
        {
            var regexArgs = match.Groups.Cast<Group>().Skip(1).Where(g => g.Success).Select(g => new ArgumentInfo() { Value = g.Value, StartOffset = g.Index });
            var arguments = regexArgs.Cast<object>().ToList();
            if (stepInstance.MultilineTextArgument != null)
                arguments.Add(stepInstance.MultilineTextArgument);
            if (stepInstance.TableArgument != null)
                arguments.Add(stepInstance.TableArgument);

            return arguments.ToArray();
        }
    }
}
