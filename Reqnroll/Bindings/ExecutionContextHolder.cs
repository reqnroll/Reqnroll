using System.Runtime.CompilerServices;
using System.Threading;

namespace Reqnroll.Bindings;

public class ExecutionContextHolder : StrongBox<ExecutionContext>
{
}
