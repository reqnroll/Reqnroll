using System;

namespace Reqnroll
{
    /// <summary>
    /// Specifies the method to be used as a custom step definition parameter conversion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class StepArgumentTransformationAttribute : Attribute
    {
        /// <summary>
        /// The regular expression that have to match the step argument. The entire argument is passed to the method if omitted.
        /// Please note that Cucumber Expressions are not supported for step argument transformations.
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// The custom parameter type name to be used in Cucumber Expressions
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the deterministic order for step argument transformations. Lower numbers have higher priority.
        /// Default value is <see cref="StepArgumentTransformationAttribute.DefaultOrder">10000</see>.
        /// </summary>
        public int Order { get; set; }
        
        public const int DefaultOrder = 10000;

        /// <summary>
        /// Create a Step Argument Transformation
        /// </summary>
        /// <param name="regex">The regular expression that must match the step argument. The entire argument is passed to the method if omitted. Please note that Cucumber Expressions are not supported for step argument transformations.</param>
        /// <param name="order">Specifies the deterministic order for step argument transformations. Lower numbers have higher priority.</param>
        public StepArgumentTransformationAttribute(string regex, int order = DefaultOrder)
        {
            Regex = regex;
            Order = order;
        }

        public StepArgumentTransformationAttribute()
        {
            Regex = null;
        }
    }
}
