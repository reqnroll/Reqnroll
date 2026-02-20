#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;

namespace Reqnroll.Formatters;

public abstract class FormatterBase : ICucumberMessageFormatter, IDisposable
{
    private Task? _formatterTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private ICucumberMessageBroker? _broker;
    private readonly IFormattersConfigurationProvider _configurationProvider;
    private readonly IFormatterLog _logger;

    protected readonly Channel<Envelope> PostedMessages = Channel.CreateUnbounded<Envelope>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = true }
    );

    private bool _isDisposed = false;
    protected bool Closed = false;

    protected virtual TimeSpan CloseAsyncTimeout => TimeSpan.FromSeconds(15);
    protected virtual TimeSpan CloseAsyncCancellationGracePeriod => TimeSpan.FromSeconds(5);
    protected virtual TimeSpan DisposeTimeout => TimeSpan.FromSeconds(15);

    public IFormatterLog Logger => _logger;
    internal IFormattersConfigurationProvider ConfigurationProvider => _configurationProvider;

    private readonly string _pluginName;

    public string Name => _pluginName;

    protected FormatterBase(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, string pluginName)
    {
        _configurationProvider = configurationProvider;
        _logger = logger;
        _pluginName = pluginName;
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} in constructor.");
    }

    public void LaunchFormatter(ICucumberMessageBroker broker)
    {
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} in Launch().");
        _broker = broker;

        bool IsFormatterEnabled(out IDictionary<string, object> configuration)
        {
            configuration = null!;
            if (!_configurationProvider.Enabled)
                return false;
            configuration = _configurationProvider.GetFormatterConfigurationByName(_pluginName);

            return configuration != null;
        }

        if (!IsFormatterEnabled(out var formatterConfiguration))
        {
            _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} disabled via configuration.");

            ReportInitialized(false);
            return;
        }
        LaunchInner(formatterConfiguration, ReportInitialized);
        _formatterTask = Task.Run(() => ConsumeAndFormatMessagesBackgroundTask(_cancellationTokenSource.Token));
    }

    // Method available to sinks to allow them to initialize.
    public abstract void LaunchInner(IDictionary<string, object> formatterConfigString, Action<bool> onAfterInitialization);

    private void ReportInitialized(bool status)
    {
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} reporting status as {status}.");
        Closed = !status;

        // Preemptively closing down the Channel to force error identification
        if (status == false)
            PostedMessages.Writer.Complete();
        _broker?.FormatterInitialized(this, enabled: status);
    }

    public async Task PublishAsync(Envelope message)
    {
        if (Closed || _isDisposed || PostedMessages.Reader.Completion.IsCompleted)
        {
            Logger.WriteMessage($"Cannot add message {message.Content().GetType().Name} to formatter {Name} - formatter is closed and not able to accept additional messages.");
            return;
        }

        await PostedMessages.Writer.WriteAsync(message);

        // If the _publisher sends the TestRunFinished message, then we can safely shut down.
        if (message.Content() is TestRunFinished)
        {
            _logger.WriteMessage($"DEBUG: Formatters.Plugin {Name} has received the TestRunFinished message and is calling CloseAsync");
            await CloseAsync();
        }
    }

    protected abstract Task ConsumeAndFormatMessagesBackgroundTask(CancellationToken cancellationToken);

    internal async Task CloseAsync()
    {
        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Close called on formatter {Name}; formatter task was launched: {_formatterTask != null}");

        if (PostedMessages.Reader.Completion.IsCompleted || _formatterTask!.IsCompleted)
            throw new InvalidOperationException($"Formatter {Name} has invoked Close when it is already in a closed state.");

        PostedMessages.Writer.Complete();

        Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} has signaled the Channel is closed. Awaiting the writing task.");

        var timeoutTask = Task.Delay(CloseAsyncTimeout);
        var finishedTask = await Task.WhenAny(timeoutTask, _formatterTask!);

        if (finishedTask == timeoutTask)
        {
            Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - Timed out waiting for formatterTask. Requesting cancellation.");
            _cancellationTokenSource.Cancel();

            var gracePeriod = Task.Delay(CloseAsyncCancellationGracePeriod);
            finishedTask = await Task.WhenAny(gracePeriod, _formatterTask!);

            if (finishedTask == gracePeriod)
            {
                Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - formatterTask did not respond to cancellation within grace period.");
            }
            else
            {
                try
                {
                    await _formatterTask!;
                    Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - formatterTask completed after cancellation.");
                }
                catch (OperationCanceledException)
                {
                    Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - formatterTask threw OperationCanceledException after cancellation (expected).");
                }
            }
        }
        else
        {
            Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - The formatterTask is now completed.");
            await _formatterTask!;
        }

        if (!PostedMessages.Reader.Completion.IsCompleted)
            throw new InvalidOperationException($"Formatter {Name} has shut down before all messages processed.");
    }

    public virtual void Dispose()
    {
        Logger.WriteMessage($"DEBUG: Formatters: Dispose called on FormatterPlugin {Name}, isDisposed: {_isDisposed}, Formatter Task was launched: {_formatterTask != null}, Queue is empty:{PostedMessages.Reader.Completion.IsCompleted}");
        if (!_isDisposed)
        {
            try
            {
                if (_formatterTask != null)
                {
                    if (!_formatterTask.IsCompleted)
                    {
                        // CloseAsync was never called (e.g. TestRunFinished was never received).
                        // Signal immediate shutdown: complete the channel and cancel the token.
                        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} still running; forcing shutdown.");
                        PostedMessages.Writer.TryComplete();
                        _cancellationTokenSource.Cancel();

                        var timeoutTask = Task.Delay(DisposeTimeout);
                        var finishedTask = Task.WhenAny(timeoutTask, _formatterTask).GetAwaiter().GetResult();
                        if (finishedTask == timeoutTask)
                        {
                            Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - timeout waiting for formatter {Name} after cancellation.");
                        }
                        else
                        {
                            Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} completed after cancellation.");
                            _formatterTask.GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} had already finished.");
                        _formatterTask.GetAwaiter().GetResult();
                    }
                }
                else
                {
                    Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - skipping wait on formatters task {Name}.");
                }
                _cancellationTokenSource.Dispose();
            }
            catch (System.Exception e)
            {
                Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose- formatter task {Name} threw Ex: {e.Message}");
            }
            finally
            {
                _logger.DumpMessages();
                _isDisposed = true;
            }
        }
    }
}