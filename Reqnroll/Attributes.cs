using System;
using Reqnroll.Bindings;

namespace Reqnroll
{
    /// <summary>
    /// Marker attribute that specifies that this class may contain bindings (step definitions, hooks, etc.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BindingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class StepDefinitionBaseAttribute : Attribute
    {
        public StepDefinitionType[] Types { get; }

        /// <summary>
        /// A cucumber expression or a regular expression (regex) that matches the step text.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Additional information in which culture the step is written. It does not affect the matching of the step, it is only for tooling support needed
        /// </summary>
        public string Culture { get; set; }

        internal StepDefinitionBaseAttribute(string expression, StepDefinitionType type)
            : this(expression, new[] { type })
        {
        }

        protected StepDefinitionBaseAttribute(string expression, StepDefinitionType[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            if (types.Length == 0) throw new ArgumentException("List cannot be empty", nameof(types));

            Expression = expression;
            Types = types;
        }
    }

    /// <summary>
    /// Specifies a 'Given' step definition that matches for the provided regular expression.
    /// </summary>
    public class GivenAttribute : StepDefinitionBaseAttribute
    {
        public GivenAttribute() : this(null)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        public GivenAttribute(string expression)
            : base(expression, StepDefinitionType.Given)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        /// <param name="culture">Additional information in which culture the step is written. It does not affect the matching of the step, it is only for tooling support needed</param>
        public GivenAttribute(string expression, string culture)
            : this(expression)
        {
            Culture = culture;
        }
    }

    /// <summary>
    /// Specifies a 'When' step definition that matches for the provided regular expression.
    /// </summary>
    public class WhenAttribute : StepDefinitionBaseAttribute
    {
        public WhenAttribute()
            : this(null)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        public WhenAttribute(string expression)
            : base(expression, StepDefinitionType.When)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        /// <param name="culture">Additional information in which culture the step is written. It does not affect the matching of the step, it is only for tooling support needed</param>
        public WhenAttribute(string expression, string culture)
            : this(expression)
        {
            Culture = culture;
        }
    }

    /// <summary>
    /// Specifies a 'Then' step definition that matches for the provided regular expression.
    /// </summary>
    public class ThenAttribute : StepDefinitionBaseAttribute
    {
        public ThenAttribute() : this(null)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        public ThenAttribute(string expression)
            : base(expression, StepDefinitionType.Then)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        /// <param name="culture">Additional information in which culture the step is written. It does not affect the matching of the step, it is only for tooling support needed</param>
        public ThenAttribute(string expression, string culture)
            : this(expression)
        {
            Culture = culture;
        }
    }

    /// <summary>
    /// Specifies a step definition that matches for the provided regular expression and any step kinds (given, when, then).
    /// </summary>
    public class StepDefinitionAttribute : StepDefinitionBaseAttribute
    {
        public StepDefinitionAttribute()
            : this(null)
        {
        }

        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        public StepDefinitionAttribute(string expression)
            : base(expression, new[] { StepDefinitionType.Given, StepDefinitionType.When, StepDefinitionType.Then })
        {
        }
        
        /// <param name="expression">A cucumber expression or a regular expression (regex) that matches the step text.</param>
        /// <param name="culture">Additional information in which culture the step is written. It does not affect the matching of the step, it is only for tooling support needed</param>
        public StepDefinitionAttribute(string expression, string culture)
            : this(expression)
        {
            Culture = culture;
        }
    }
}