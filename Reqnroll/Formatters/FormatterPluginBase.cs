#nullable enable

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Formatters;

public abstract class FormatterPluginBase : ICucumberMessageSink, IRuntimePlugin, IDisposable
{
    private Task? _formatterTask;
    internal CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly ICucumberMessageBroker _broker;
    private readonly IFormattersConfigurationProvider _configurationProvider;
    private readonly IFormatterLog _logger;
    protected readonly BlockingCollection<Envelope> PostedMessages = new();

    private bool _isDisposed = false;

    public IFormatterLog Logger { get => _logger; }
    private string _pluginName;

    public string Name => _pluginName;

    protected FormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, string pluginName)
    {
        _broker = broker;
        _configurationProvider = configurationProvider;
        _logger = logger;
        _pluginName = pluginName;
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} in constructor.");
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        LaunchSink();
    }

    internal void LaunchSink()
    {
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} in Launch().");

        bool IsFormatterEnabled(out IDictionary<string, object> configuration)
        {
            configuration = null!;
            if (!_configurationProvider.Enabled)
                return false;
            configuration = _configurationProvider.GetFormatterConfigurationByName(_pluginName);

            return configuration != null && configuration.Count > 0;
        }

        if (!IsFormatterEnabled(out var formatterConfiguration))
        {
            _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} disabled via configuration.");

            ReportInitialized(false);
            return;
        }

        _formatterTask = Task.Run(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration, ReportInitialized, _cancellationTokenSource.Token));
    }

    private void ReportInitialized(bool status)
    {
        _logger.WriteMessage($"DEBUG: Formatters: Formatter plugin: {Name} reporting status as {status}.");

        // Preemptively closing down the BlockingCollection to force error identification
        if (status == false)
            PostedMessages.CompleteAdding();
        _broker.SinkInitialized(this, enabled: status);
    }

    public async Task PublishAsync(Envelope message)
    {
        if (_isDisposed || PostedMessages.IsAddingCompleted)
        {
            Logger.WriteMessage($"Cannot add message {message.Content().GetType().Name} to formatter {Name} - formatter is closed and not able to accept additional messages.");
            return;
        }

        PostedMessages.Add(message);

        // If the _publisher sends the TestRunFinished message, then we can safely shut down.
        if (message.Content() is TestRunFinished)
        {
            _logger.WriteMessage($"DEBUG: Formatters.Plugin {Name} has recieved the TestRunFinished message and is calling CloseAsync");
            // using ConfigureAwait(false) per guidance here: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007
            await CloseAsync().ConfigureAwait(false);
        }
    }

    protected abstract Task ConsumeAndFormatMessagesBackgroundTask(IDictionary<string, object> formatterConfigString, Action<bool> onAfterInitialization, CancellationToken cancellationToken);

    internal async Task CloseAsync()
    {
        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Close called on formatter {Name}; formatter task was launched: {_formatterTask != null}");

        if (PostedMessages.IsAddingCompleted || _formatterTask!.IsCompleted)
            throw new InvalidOperationException($"Formatter {Name} has invoked Close when it is already in a closed state.");

        if (!PostedMessages.IsAddingCompleted)
            PostedMessages.CompleteAdding();
        Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} has signaled the BlockignCollection is closed. Awaiting the writing task.");
        //// using ConfigureAwait(false) per guidance here: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2007
        //var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
        //var finishedTask = await Task.WhenAny(timeoutTask, _formatterTask);
        //if (finishedTask == timeoutTask)
        //{
        //    Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Close - timeout waiting for formatter {Name}");
        //}
        //else
        //{
        //    Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Close - formatter {Name} has finished.");

        //    // The formatter task completed before timeout, allow propogation of exceptions from the Task
        //    await _formatterTask!.ConfigureAwait(false);
        //}
        await _formatterTask!.ConfigureAwait(false);
        Logger.WriteMessage($"DEBUG: Formatters:PluginBase {Name} - The formatterTask is now completed.");
        if (!PostedMessages.IsCompleted)
            throw new InvalidOperationException($"Formatter {Name} has shut down before all messages processed.");
    }

    public void Dispose()
    {
        Logger.WriteMessage($"DEBUG: Formatters: Dispose called on FormatterPlugin {Name}, isDisposed: {_isDisposed}, Formatter Task was launched: {_formatterTask != null}, Queue marked for completion: {PostedMessages.IsAddingCompleted}, Queue is empty:{PostedMessages.IsCompleted}");
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
                            if (!PostedMessages.IsAddingCompleted)
                            {
                                _logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - sending cancellation token message");
                                PostedMessages.Add(Envelope.Create(new TestRunFinished("forced closure", false, new Timestamp(0, 0), null, "")));
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

                            // The formatter task completed before timeout, allow propogation of exceptions from the Task
                            _formatterTask?.GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - formatter {Name} had already finished.");

                        // The formatter task completed before timeout, allow propogation of exceptions from the Task
                        _formatterTask?.GetAwaiter().GetResult();
                    }
                }
                else
                {
                    Logger.WriteMessage($"DEBUG: Formatters:PluginBase.Dispose - skipping wait on formatters task {Name}.");
                }
            }
            catch (System.Exception e)
            {
                Logger.WriteMessage($"DEBUG: Forrmatters:PluginBase.Dispose- formatter task {Name} threw Ex: {e.Message}");
            }
            finally
            {
                PostedMessages.Dispose();
                _isDisposed = true;
            }
        }
    }
}