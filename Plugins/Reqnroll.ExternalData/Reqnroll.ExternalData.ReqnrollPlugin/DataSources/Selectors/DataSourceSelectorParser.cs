namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources.Selectors
{
    public class DataSourceSelectorParser
    {
        public DataSourceSelector Parse(string selectorExpression)
        {
            return new FieldNameSelector(selectorExpression);
        }
    }
}
