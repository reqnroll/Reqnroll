using System;
using Reqnroll.Bindings;

namespace Reqnroll
{
    public enum ExpressionType
    {
        /// <summary>
        /// Detect Automatically if the expression is a Regular expression (Regex) or a Cucumber expression
        /// </summary>
        Automatic,
        /// <summary>
        /// The expression is a Cucumber Expression
        /// </summary>
        CucumberExpression,
        /// <summary>
        /// The expression is a Regular expression (Regex)
        /// </summary>
        RegularExpression
    }

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
        /// additional information in which culture the step is written
        /// it does not affect the matching of the step
        /// it is only for tooling support needed
        /// </summary>
        public string Culture { get; set; }

        public ExpressionType ExpressionType { get; set; } = ExpressionType.Automatic;

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


        public GivenAttribute(string expression)
            : base(expression, StepDefinitionType.Given)
        {
        }

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

        public WhenAttribute(string expression)
            : base(expression, StepDefinitionType.When)
        {
        }

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

        public ThenAttribute(string expression)
            : base(expression, StepDefinitionType.Then)
        {
        }

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

        public StepDefinitionAttribute(string expression)
            : base(expression, new[] { StepDefinitionType.Given, StepDefinitionType.When, StepDefinitionType.Then })
        {
        }

        public StepDefinitionAttribute(string expression, string culture)
            : this(expression)
        {
            Culture = culture;
        }
    }
}