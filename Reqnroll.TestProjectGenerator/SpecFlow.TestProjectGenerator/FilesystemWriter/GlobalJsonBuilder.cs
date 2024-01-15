using System.IO;
using Newtonsoft.Json;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public class GlobalJsonBuilder
    {
        public NetCoreSdkInfo Sdk { get; private set; }

        public ProjectFile ToProjectFile()
        {
            using (var stringWriter = new StringWriter())
            {
                WriteJsonToTextWriter(stringWriter);
                return new ProjectFile("global.json", "None", stringWriter.ToString());
            }
        }

        public void WriteJsonToTextWriter(TextWriter textWriter)
        {
            using (var jsonTextWriter = new JsonTextWriter(textWriter))
            {
                jsonTextWriter.WriteToken(JsonToken.StartObject);

                jsonTextWriter.WritePropertyName("sdk", true);
                jsonTextWriter.WriteToken(JsonToken.StartObject);
                jsonTextWriter.WritePropertyName("version", true);
                jsonTextWriter.WriteValue(Sdk.Version);

                jsonTextWriter.WritePropertyName("rollForward");
                jsonTextWriter.WriteValue("latestFeature");

                jsonTextWriter.WriteToken(JsonToken.EndObject);

                jsonTextWriter.WriteToken(JsonToken.EndObject);
            }
        }

        public GlobalJsonBuilder WithSdk(NetCoreSdkInfo sdk)
        {
            Sdk = sdk;
            return this;
        }
    }
}
