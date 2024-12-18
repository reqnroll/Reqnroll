using Microsoft.Extensions.Logging;

namespace Reqnroll.Microsoft.Extensions.Logging;

public sealed class ReqnrollLogger<T>(IReqnrollOutputHelper outputHelper, LoggerExternalScopeProvider scopeProvider) :
    ReqnrollLogger(outputHelper, scopeProvider, typeof(T).FullName), ILogger<T>;