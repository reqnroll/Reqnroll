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
        /// </summary>
        public string Regex { get; set; }
        /// <summary>
        /// The custom parameter type name to be used in Cucumber Expressions
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the deterministic order for step argument transformations. Lower numbers have higher priority.
        /// Before .NET 7, step argument transformations with the same priority will execute in a non-deterministic order.
        /// Default value is 0.
        /// </summary>
        public int Order { get; set; }

        public StepArgumentTransformationAttribute(string regex, int order = default)
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
