using System.Collections.Generic;
using Reqnroll.ExternalData.ReqnrollPlugin.DataSources.Selectors;

namespace Reqnroll.ExternalData.ReqnrollPlugin.DataSources
{
    public class DataRecord
    {
        public IDictionary<string, DataValue> Fields { get; } = new Dictionary<string, DataValue>(FieldNameComparer.Value);

        public DataRecord()
        {
            
        }

        public DataRecord(Dictionary<string, string> fields)
        {
            foreach (var field in fields)
            {
                Fields.Add(field.Key, new DataValue(field.Value));
            }
        }

        public DataRecord(IEnumerable<KeyValuePair<string, DataValue>> fields)
        {
            foreach (var field in fields)
            {
                Fields.Add(field.Key, field.Value);
            }
        }
    }
}
