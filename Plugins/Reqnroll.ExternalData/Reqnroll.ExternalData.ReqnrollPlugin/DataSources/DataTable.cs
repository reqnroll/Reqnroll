using System;
using System.Collections.Generic;

namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources
{
    public class DataTable
    {
        public string[] Header { get; }
        public IList<DataRecord> Items { get; } = new List<DataRecord>();

        public DataTable(string[] header)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
        }
    }
}
