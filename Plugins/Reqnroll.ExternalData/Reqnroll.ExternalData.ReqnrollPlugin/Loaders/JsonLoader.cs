using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Reqnroll.ExternalData.ReqnrollPlugin.DataSources;
using Newtonsoft.Json;

namespace Reqnroll.ExternalData.ReqnrollPlugin.Loaders
{
    public class JsonLoader : FileBasedLoader
    {
        public JsonLoader() : base("Json", ".json")
        {

        }

        protected override DataSource LoadDataSourceFromFilePath(string filePath, string sourceFilePath)
        {
            var fileContent = ReadTextFileContent(filePath);

            return LoadJsonDataSource(fileContent, sourceFilePath);
        }

        private List<string> DetermineDataSets(JObject jsonObject, string dataSetPrepend)
        {
            var dataSets = new List<string>();
            foreach (var array in GetArraysFromObject(jsonObject))
            {
                var firstArrayEntry = array.Children().FirstOrDefault();

                if (firstArrayEntry == null || firstArrayEntry.Type != JTokenType.Object)
                    continue;
                
                var firstArrayObject = firstArrayEntry.ToObject<JObject>();

                var dataSetPath = string.IsNullOrWhiteSpace(dataSetPrepend) ? ((JProperty)array.Parent!).Name : $"{dataSetPrepend}.{array.Path}";

                dataSets.Add(dataSetPath);

                foreach (var nestedDataSetPath in DetermineDataSets(firstArrayObject, dataSetPath))
                    dataSets.Add(nestedDataSetPath);
            }

            return dataSets;
        }

        private static IEnumerable<JToken> GetArraysFromObject(JObject jsonObject)
        {
            return jsonObject.Properties().Where(p => p.Value.Type == JTokenType.Array).Select(a => a.Value);
        }

        private DataSource LoadJsonDataSource(string fileContent, string sourceFilePath)
        {
            JObject fileJson = ParseJson(fileContent, sourceFilePath);
            var dataSets = DetermineDataSets(fileJson, "");
            var dataSetsRecord = new DataRecord();
            var jsonDataTableGenerator = new JsonDataTableGenerator();
            foreach (var dataSetPath in dataSets)
            {
                var dataTable = jsonDataTableGenerator.FlattenDataSetToDataTable(fileJson, dataSetPath.Split('.'));
                dataSetsRecord.Fields[dataSetPath] = new DataValue(dataTable);
            }

            return new DataSource(dataSetsRecord, dataSets.First());
        }

        private static JObject ParseJson(string fileContent, string sourceFilePath)
        {
            JObject fileJson;
            try
            {
                fileJson = JObject.Parse(fileContent);
            }
            catch (JsonReaderException jsonReaderException)
            {
                throw new ExternalDataPluginException($"Failed to parse json file {sourceFilePath}", jsonReaderException);
            }

            return fileJson;
        }
    }
}