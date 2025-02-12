using Microsoft.Extensions.Logging;

namespace Reqnroll.Microsoft.Extensions.DependencyInjection.Logging;

public sealed class ReqnrollLoggerProvider : ILoggerProvider
{
    private readonly IReqnrollOutputHelper _outputHelper;
    private readonly ReqnrollLoggerOptions _options;
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public ReqnrollLoggerProvider(IReqnrollOutputHelper outputHelper, bool appendScope)
        : this(outputHelper, new ReqnrollLoggerOptions { IncludeScopes = appendScope })
    {
    }

    public ReqnrollLoggerProvider(IReqnrollOutputHelper outputHelper, ReqnrollLoggerOptions? options = null)
    {
        _outputHelper = outputHelper;
        _options = options ?? new ReqnrollLoggerOptions();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ReqnrollLogger(_outputHelper, _scopeProvider, categoryName, _options);
    }

    public void Dispose()
    {
    }
}