namespace Reqnroll.TestProjectGenerator.Data
{
    public class NuGetSource
    {
        public NuGetSource(string key, string value, string apiKey = null)
        {
            Key = key;
            Value = value;
            APIKey = apiKey;
        }

        public string Key { get; }
        public string Value { get; }
        public string APIKey { get; }
    }
}