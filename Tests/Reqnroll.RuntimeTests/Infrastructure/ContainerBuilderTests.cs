using System.Linq.Expressions;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void InitializeContainerBuilder_Does_Not_Throw_Exception_For_DynamicAssemblies()
        {
            //ARRANGE
            var containerBuilder = new RuntimeTestsContainerBuilder();
            var dynamicAssembly = Expression.Lambda(Expression.Empty()).Compile().Method.Module.Assembly;

            //ACT
            containerBuilder.CreateGlobalContainer(dynamicAssembly);

            //ASSERT NO EXCEPTION
        }
    }
}
