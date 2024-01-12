namespace Reqnroll.Assist.ValueRetrievers
{
    public class StringValueRetriever : ClassRetriever<string>
    {
        protected override string GetNonEmptyValue(string value)
        {
            return value;
        }
    }
}