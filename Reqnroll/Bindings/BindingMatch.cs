using Reqnroll.Infrastructure;
using System.Linq;

namespace Reqnroll.Bindings
{
    public class BindingMatch
    {
        public static readonly BindingMatch NonMatching = new BindingMatch(null, 0, null, null);

        public IStepDefinitionBinding StepBinding { get; private set; }
        public bool Success { get { return StepBinding != null; } }

        public BindingObsoletion BindingObsoletion { get; private set; }
        public bool IsObsolete => BindingObsoletion.IsObsolete;

        public int ScopeMatches { get; private set; }
        public bool IsScoped { get { return ScopeMatches > 0; } }

        public object[] Arguments  { get; private set; }
        public int?[] ArgumentStartOffsets { get; private set; }
        public StepContext StepContext { get; private set; }

        public BindingMatch(IStepDefinitionBinding stepBinding, int scopeMatches, MatchArgument[] arguments, StepContext stepContext)
        {
            StepBinding = stepBinding;
            ScopeMatches = scopeMatches;
            Arguments = arguments == null ? null : arguments.Select(a => a.Value).ToArray();
            ArgumentStartOffsets = arguments == null ? null : arguments.Select(a => a.StartOffset).ToArray(); 
            StepContext = stepContext;
            BindingObsoletion = new BindingObsoletion(stepBinding);
        }
    }
}