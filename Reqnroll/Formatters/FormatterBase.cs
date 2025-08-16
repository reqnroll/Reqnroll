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

    public IFormatterLog Logger => _logger;

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
        await _formatterTask!;
        Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - The formatterTask is now completed.");

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
                        Logger.WriteMessage($"DEBUG: Formatters: Dispose is waiting on the formatter task {Name}.");
                        // In this situation, the TestEngine is shutting down and has called Dispose on the global container.
                        // Forcing the Dispose to wait until the formatter has had a chance to complete.
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                        var finishedTask = Task.WhenAny(timeoutTask, _formatterTask).GetAwaiter().GetResult();
                        if (finishedTask == timeoutTask)
                        {
                            Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - timeout waiting for formatter {Name}");
                            _cancellationTokenSource.Cancel();
                            // the following sends a simulated message to the writer, which will then notice the cancellation token
                            if (!PostedMessages.Reader.Completion.IsCompleted)
                            {
                                _logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - sending cancellation token message");
                                PostedMessages.Writer.TryWrite(Envelope.Create(new TestRunFinished("forced closure", false, new Timestamp(0, 0), null, "")));
                            }
                            else
                            {
                                _logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - cancellation message can't be sent as the collection is closed.");
                            }
                            _logger.WriteMessage($"DEBUG: Formatters.PluginBase.Dispose - waiting again after cancellation.");
                            timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                            finishedTask = Task.WhenAny(timeoutTask, _formatterTask).GetAwaiter().GetResult();
                            if (finishedTask == timeoutTask)
                            {
                                _logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - cancellation unsuccessful.");
                            }
                            else
                            {
                                _logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - cancellation successful.");
                                _formatterTask?.GetAwaiter().GetResult();
                            }
                        }
                        else
                        {
                            Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} has finished.");

                            // The formatter task completed before timeout, allow propagation of exceptions from the Task
                            _formatterTask?.GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} had already finished.");

                        // The formatter task completed before timeout, allow propagation of exceptions from the Task
                        _formatterTask?.GetAwaiter().GetResult();
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