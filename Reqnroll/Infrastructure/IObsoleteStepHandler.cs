using Reqnroll.Bindings;

namespace Reqnroll.Infrastructure
{
    public interface IObsoleteStepHandler
    {
        void Handle(BindingMatch bindingMatch);
    }
}