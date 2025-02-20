using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection;

[Serializable]
public class InvalidScenarioDependenciesException(string message) : ReqnrollException(message)
{
    public override string HelpLink { get; set; } = "https://go.reqnroll.net/doc-msdi";
}