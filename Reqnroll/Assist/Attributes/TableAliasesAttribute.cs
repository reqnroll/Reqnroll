using System;

namespace Reqnroll.Assist.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = true)]
    public class TableAliasesAttribute : Attribute
    {
        public TableAliasesAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public TableAliasesAttribute(bool useExactMatch, params string[] aliases)
        {
            UseExactMatch = useExactMatch;
            Aliases = aliases;
        }

        public string[] Aliases { get; }
        public bool UseExactMatch { get; }
    }
}