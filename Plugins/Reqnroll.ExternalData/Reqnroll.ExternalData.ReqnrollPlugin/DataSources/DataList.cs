using System.Collections.Generic;

namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources
{
    // Not used currently, will be needed for hierarchical data sets.
    public class DataList
    {
        public IList<DataValue> Items { get; } = new List<DataValue>();
    }
}
