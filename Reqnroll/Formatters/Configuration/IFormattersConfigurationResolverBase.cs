﻿using System.Collections.Generic;

namespace Reqnroll.Formatters.Configuration;

public interface IFormattersConfigurationResolverBase
{
    IDictionary<string, IDictionary<string, object>> Resolve();
}

public interface IFormattersConfigurationResolver : IFormattersConfigurationResolverBase;

public interface IFormattersEnvironmentOverrideConfigurationResolver : IFormattersConfigurationResolverBase;