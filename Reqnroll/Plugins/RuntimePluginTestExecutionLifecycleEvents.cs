using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;

namespace Reqnroll.Plugins
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs);

    public class RuntimePluginTestExecutionLifecycleEvents
    {
        public event EventHandler<RuntimePluginBeforeTestRunEventArgs> BeforeTestRun;
        public event AsyncEventHandler<RuntimePluginBeforeTestRunEventArgs> BeforeTestRunAsync;

        public event EventHandler<RuntimePluginAfterTestRunEventArgs> AfterTestRun;
        public event AsyncEventHandler<RuntimePluginAfterTestRunEventArgs> AfterTestRunAsync;

        public event EventHandler<RuntimePluginBeforeFeatureEventArgs> BeforeFeature;
        public event AsyncEventHandler<RuntimePluginBeforeFeatureEventArgs> BeforeFeatureAsync;

        public event EventHandler<RuntimePluginAfterFeatureEventArgs> AfterFeature;
        public event AsyncEventHandler<RuntimePluginAfterFeatureEventArgs> AfterFeatureAsync;

        public event EventHandler<RuntimePluginBeforeScenarioEventArgs> BeforeScenario;
        public event AsyncEventHandler<RuntimePluginBeforeScenarioEventArgs> BeforeScenarioAsync;

        public event EventHandler<RuntimePluginAfterScenarioEventArgs> AfterScenario;
        public event AsyncEventHandler<RuntimePluginAfterScenarioEventArgs> AfterScenarioAsync;

        public event EventHandler<RuntimePluginBeforeStepEventArgs> BeforeStep;
        public event AsyncEventHandler<RuntimePluginBeforeStepEventArgs> BeforeStepAsync;

        public event EventHandler<RuntimePluginAfterStepEventArgs> AfterStep;
        public event AsyncEventHandler<RuntimePluginAfterStepEventArgs> AfterStepAsync;

        private async Task InvokeAsyncEventHandler<TEventArgs>(AsyncEventHandler<TEventArgs> asyncEventHandler, TEventArgs eventArgs)
        {
            if (asyncEventHandler is null)
                return;
            foreach (var subscriber in asyncEventHandler.GetInvocationList())
            {
                var asyncSubscriber = (AsyncEventHandler<TEventArgs>)subscriber;
                await asyncSubscriber.Invoke(this, eventArgs);
            }
        }

        public async Task RaiseBeforeTestRunAsync(IObjectContainer objectContainer)
        {
            if (BeforeTestRun is null && BeforeTestRunAsync is null)
                return;
            var args = new RuntimePluginBeforeTestRunEventArgs(objectContainer);
            BeforeTestRun?.Invoke(this, args);
            await InvokeAsyncEventHandler(BeforeTestRunAsync, args);
        }

        public async Task RaiseAfterTestRunAsync(IObjectContainer objectContainer)
        {
            if (AfterTestRun is null && AfterTestRunAsync is null)
                return;
            var args = new RuntimePluginAfterTestRunEventArgs(objectContainer);
            AfterTestRun?.Invoke(this, args);
            await InvokeAsyncEventHandler(AfterTestRunAsync, args);
        }

        public async Task RaiseBeforeFeatureAsync(IObjectContainer objectContainer)
        {
            if (BeforeFeature is null && BeforeFeatureAsync is null)
                return;
            var args = new RuntimePluginBeforeFeatureEventArgs(objectContainer);
            BeforeFeature?.Invoke(this, args);
            await InvokeAsyncEventHandler(BeforeFeatureAsync, args);
        }

        public async Task RaiseAfterFeatureAsync(IObjectContainer objectContainer)
        {
            if (AfterFeature is null && AfterFeatureAsync is null)
                return;
            var args = new RuntimePluginAfterFeatureEventArgs(objectContainer);
            AfterFeature?.Invoke(this, args);
            await InvokeAsyncEventHandler(AfterFeatureAsync, args);
        }

        public async Task RaiseBeforeScenarioAsync(IObjectContainer objectContainer)
        {
            if (BeforeScenario is null && BeforeScenarioAsync is null)
                return;
            var args = new RuntimePluginBeforeScenarioEventArgs(objectContainer);
            BeforeScenario?.Invoke(this, args);
            await InvokeAsyncEventHandler(BeforeScenarioAsync, args);
        }

        public async Task RaiseAfterScenarioAsync(IObjectContainer objectContainer)
        {
            if (AfterScenario is null && AfterScenarioAsync is null)
                return;
            var args = new RuntimePluginAfterScenarioEventArgs(objectContainer);
            AfterScenario?.Invoke(this, args);
            await InvokeAsyncEventHandler(AfterScenarioAsync, args);
        }

        public async Task RaiseBeforeStepAsync(IObjectContainer objectContainer)
        {
            if (BeforeStep is null && BeforeStepAsync is null)
                return;
            var args = new RuntimePluginBeforeStepEventArgs(objectContainer);
            BeforeStep?.Invoke(this, args);
            await InvokeAsyncEventHandler(BeforeStepAsync, args);
        }

        public async Task RaiseAfterStepAsync(IObjectContainer objectContainer)
        {
            if (AfterStep is null && AfterStepAsync is null)
                return;
            var args = new RuntimePluginAfterStepEventArgs(objectContainer);
            AfterStep?.Invoke(this, args);
            await InvokeAsyncEventHandler(AfterStepAsync, args);
        }
    }
}
