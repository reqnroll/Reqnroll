using System.Collections.Generic;

namespace Reqnroll.Bindings;

public interface IStepDefinitionBindingBuilder
{
    public IEnumerable<IStepDefinitionBinding> Build();
}