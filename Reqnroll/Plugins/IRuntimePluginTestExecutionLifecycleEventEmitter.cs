using Reqnroll.BoDi;
using Reqnroll.Bindings;
using System.Threading.Tasks;

namespace Reqnroll.Plugins
{
    public interface IRuntimePluginTestExecutionLifecycleEventEmitter
    {
        Task RaiseExecutionLifecycleEventAsync(HookType hookType, IObjectContainer container);
    }

    public class RuntimePluginTestExecutionLifecycleEventEmitter : IRuntimePluginTestExecutionLifecycleEventEmitter
    {
        private readonly RuntimePluginTestExecutionLifecycleEvents _events;

        public RuntimePluginTestExecutionLifecycleEventEmitter(RuntimePluginTestExecutionLifecycleEvents events)
        {
            _events = events;
        }

        public async Task RaiseExecutionLifecycleEventAsync(HookType hookType, IObjectContainer container)
        {
            switch (hookType)
            {
                case HookType.BeforeTestRun:
                    await _events.RaiseBeforeTestRunAsync(container);
                    break;
                case HookType.AfterTestRun:
                    await _events.RaiseAfterTestRunAsync(container);
                    break;
                case HookType.BeforeFeature:
                    await _events.RaiseBeforeFeatureAsync(container);
                    break;
                case HookType.AfterFeature:
                    await _events.RaiseAfterFeatureAsync(container);
                    break;
                case HookType.BeforeScenario:
                    await _events.RaiseBeforeScenarioAsync(container);
                    break;
                case HookType.AfterScenario:
                    await _events.RaiseAfterScenarioAsync(container);
                    break;
                case HookType.BeforeStep:
                    await _events.RaiseBeforeStepAsync(container);
                    break;
                case HookType.AfterStep:
                    await _events.RaiseAfterStepAsync(container);
                    break;
                default: break;
            }

        }
    }
}
