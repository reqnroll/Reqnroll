using System.Text.RegularExpressions;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.Bindings
{
    public class StepArgumentTransformationBinding : MethodBinding, IStepArgumentTransformationBinding
    {
        public string Name { get; }

        public Regex Regex { get; }
        
        public int Order { get; }

        public StepArgumentTransformationBinding(Regex regex, IBindingMethod bindingMethod, string name = null, 
            int order = StepArgumentTransformationAttribute.DefaultOrder)
            : base(bindingMethod)
        {
            Regex = regex;
            Name = name;
            Order = order;
        }

        public StepArgumentTransformationBinding(string regexString, IBindingMethod bindingMethod, string name = null, 
            int order = StepArgumentTransformationAttribute.DefaultOrder)
            : this(CreateRegexOrNull(regexString), bindingMethod, name, order)
        {
        }

        private static Regex CreateRegexOrNull(string regexString)
        {
            if (regexString == null)
                return null;
            return RegexFactory.CreateWholeTextRegexForBindings(regexString);
        }
    }
}
