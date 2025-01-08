﻿using System;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection
{
    [Serializable]
    public class MissingScenarioDependenciesException : ReqnrollException
    {
        public MissingScenarioDependenciesException()
            : base("No method marked with [ScenarioDependencies] attribute found, or unsupported return type specified.")
        {
            HelpLink = "https://go.reqnroll.net/doc-msdi";
        }
    }
}
