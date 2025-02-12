namespace Reqnroll.Bindings
{
    public class BindingMatch
    {
        public static readonly BindingMatch NonMatching = new BindingMatch(null, 0, null, null, null);

        public IStepDefinitionBinding StepBinding { get; private set; }
        public bool Success { get { return StepBinding != null; } }

        public BindingObsoletion BindingObsoletion { get; private set; }
        public bool IsObsolete => BindingObsoletion.IsObsolete;

        public int ScopeMatches { get; private set; }
        public bool IsScoped { get { return ScopeMatches > 0; } }

        public object[] Arguments  { get; private set; }
        public int?[] ArgumentStartOffsets { get; private set; }
        public StepContext StepContext { get; private set; }

        public BindingMatch(IStepDefinitionBinding stepBinding, int scopeMatches, object[] arguments, int?[] argumentStartOffsets, StepContext stepContext)
        {
            StepBinding = stepBinding;
            ScopeMatches = scopeMatches;
            Arguments = arguments;
            ArgumentStartOffsets = argumentStartOffsets;
            StepContext = stepContext;
            BindingObsoletion = new BindingObsoletion(stepBinding);
        }
    }
}