using System;
using Reqnroll.Configuration.JsonConfig;

namespace Reqnroll.Configuration;
public class JsonStringRuntimeConfigurationProvider(string jsonConfigFileContent) : IRuntimeConfigurationProvider
{
    public ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration)
    {
        if (reqnrollConfiguration == null)
            throw new ArgumentNullException(nameof(reqnrollConfiguration));

        if (jsonConfigFileContent == null) return reqnrollConfiguration;

        if (!jsonConfigFileContent.Trim().StartsWith("{"))
            throw new NotSupportedException($"Only JSON config value can be provided! Provided value: {jsonConfigFileContent}");

        var jsonConfigurationLoader = new JsonConfigurationLoader();
        return jsonConfigurationLoader.LoadJson(reqnrollConfiguration, jsonConfigFileContent);
    }
}
