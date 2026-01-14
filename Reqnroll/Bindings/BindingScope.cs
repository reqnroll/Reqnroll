using System;
using System.Linq;
using Cucumber.TagExpressions;
using Reqnroll.Bindings.Discovery;

namespace Reqnroll.Bindings
{
    public class BindingScope(ITagExpression tagExpression, string featureTitle, string scenarioTitle)
    {
        public string Tag => tagExpression.ToString();
        public ITagExpression TagExpression => tagExpression;

        public string FeatureTitle { get; } = featureTitle;

        public string ScenarioTitle { get; } = scenarioTitle;
        public bool IsValid => ErrorMessage == null;
        public string ErrorMessage => tagExpression is InvalidTagExpression ? tagExpression.ToString() : null;

        public bool Match(StepContext stepContext, out int scopeMatches)
        {
            scopeMatches = 0;

            if (tagExpression is not NullExpression)
            {
                var tags = stepContext.Tags.Select(t => "@" + t).ToList();

                if (!tagExpression.Evaluate(tags))    
                    return false;

                scopeMatches++;
            }
            if (FeatureTitle != null)
            {
                if (!string.Equals(FeatureTitle, stepContext.FeatureTitle, StringComparison.CurrentCultureIgnoreCase))
                    return false;

                scopeMatches++;
            }
            if (ScenarioTitle != null)
            {
                if (!string.Equals(ScenarioTitle, stepContext.ScenarioTitle, StringComparison.CurrentCultureIgnoreCase))
                    return false;

                scopeMatches++;
            }

            return true;
        }

        protected bool Equals(BindingScope other)
        {
            return string.Equals(Tag, other.Tag) && string.Equals(FeatureTitle, other.FeatureTitle) && string.Equals(ScenarioTitle, other.ScenarioTitle) && string.Equals(ErrorMessage, other.ErrorMessage);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BindingScope) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Tag != null ? Tag.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FeatureTitle != null ? FeatureTitle.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ScenarioTitle != null ? ScenarioTitle.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ErrorMessage != null ? ErrorMessage.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
