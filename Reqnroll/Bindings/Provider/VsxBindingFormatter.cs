#nullable enable
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Infrastructure;
using Reqnroll.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Bindings.Provider;

/// <summary>
/// An internal formatter that writes the discovered bindings to a JSON file for the Visual Studio extension.
/// </summary>
public class VsxBindingFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem, IBindingRegistry bindingRegistry, ITestAssemblyProvider testAssemblyProvider)
    : FileWritingFormatterBase(configurationProvider, logger, fileSystem, FormatterName, ".json", "reqnroll_bindings.json")
{
    internal const string FormatterName = "vsxbinding";

    private TextWriter? _outputWriter;

    protected override void OnTargetFileStreamInitialized(Stream targetFileStream)
    {
        _outputWriter = new StreamWriter(targetFileStream) { AutoFlush = true };
    }

    protected override void OnTargetFileStreamDisposing()
    {
        _outputWriter?.Dispose();
        _outputWriter = null;
    }

    protected override async Task WriteToFile(Envelope envelope, CancellationToken cancellationToken)
    {
        if (_outputWriter == null)
            return;

        if (envelope.TestRunStarted != null && bindingRegistry.Ready)
        {
            var resultData = BindingProviderService.GetDiscoveredBindingsFromRegistry(bindingRegistry, testAssemblyProvider.TestAssembly);
            await _outputWriter.WriteAsync(resultData);
        }
    }
}