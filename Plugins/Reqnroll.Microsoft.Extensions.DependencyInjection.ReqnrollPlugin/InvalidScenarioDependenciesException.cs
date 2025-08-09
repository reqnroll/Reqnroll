using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection;

public class InvalidScenarioDependenciesException(string reason) : ReqnrollException("[ScenarioDependencies] should return IServiceCollection but " + reason)
{
    public override string HelpLink { get; set; } = "https://go.reqnroll.net/doc-msdi";
}