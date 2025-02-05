namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources
{
    public class DataSource : DataValue
    {
        public string DefaultDataSet { get; }

        public DataSource(object value, string defaultDataSet = null) : base(value)
        {
            DefaultDataSet = defaultDataSet;
        }
    }
}
