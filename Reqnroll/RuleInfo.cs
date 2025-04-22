using System;
using System.Diagnostics;

namespace Reqnroll
{
    /// <summary>
    /// Contains information about the rule currently being executed.
    /// </summary>
    [DebuggerDisplay("{Title}")]
    public class RuleInfo
    {
        /// <summary>
        /// The title (name) of the rule.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The description of the rule.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Direct tags of the rule.
        /// </summary>
        public string[] Tags { get; }

        public RuleInfo(string title, string description, string[] tags)
        {
            Title = title;
            Description = description;
            Tags = tags ?? Array.Empty<string>();
        }
    }
}