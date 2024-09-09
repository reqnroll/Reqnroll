using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cucumber.Messages;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace CucumberMessages.CompatibilityTests
{
    public class FluentAsssertionCucumberMessagePropertySelectionRule : IMemberSelectionRule
    {
        public FluentAsssertionCucumberMessagePropertySelectionRule(IEnumerable<Type> CucumberMessageTypes) 
        { this.CucumberMessageTypes = CucumberMessageTypes; }

        public IEnumerable<Type> CucumberMessageTypes { get; }

        public bool IncludesMembers => false;

        public IEnumerable<IMember> SelectMembers(INode currentNode, IEnumerable<IMember> selectedMembers, MemberSelectionContext context)
        {
            if (CucumberMessageTypes.Contains(context.Type))
            {
                var propertiesToSelect = new List<IMember>();
                foreach (var prop in selectedMembers)
                {
                    if (prop.Name != "Id" && prop.Name != "Location" && prop.Name != "Uri" )
                        propertiesToSelect.Add(prop);
                }
                return propertiesToSelect;
            }
            else
            {
                return selectedMembers;
            }
        }
        public override string ToString()
        {
            return "Include only relevant CucumberMessage properties";
        }

    }
}
