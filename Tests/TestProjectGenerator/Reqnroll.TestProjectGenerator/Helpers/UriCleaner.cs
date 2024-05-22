using System;

namespace Reqnroll.TestProjectGenerator.Helpers
{
    public class UriCleaner 
    {
        public string StripSchema(string uriString)
        {
            var uri = new Uri(uriString);
            var schema = uri.Scheme;

            return uriString.Remove(0, schema.Length + 1).TrimStart('/');
        }

        public string ConvertSlashes(string actualUri)
        {
            return actualUri.Replace('/', '\\');
        }
    }
}