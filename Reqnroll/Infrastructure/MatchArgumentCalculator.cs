using Reqnroll.Bindings;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.Infrastructure
{
    public interface IMatchArgumentCalculator
    {
        object[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding);
    }

    public class MatchArgumentCalculator : IMatchArgumentCalculator
    {
        public object[] CalculateArguments(Match match, StepInstance stepInstance, IStepDefinitionBinding stepDefinitionBinding)
        {
            var regexArgs = match.Groups.Cast<Group>().Skip(1).Where(g => g.Success).Select(g => g.Value);
            var arguments = regexArgs.Cast<object>().ToList();
            if (stepInstance.MultilineTextArgument != null)
                arguments.Add(stepInstance.MultilineTextArgument);
            if (stepInstance.TableArgument != null)
                arguments.Add(stepInstance.TableArgument);

            return arguments.ToArray();
        }
    }
}
