using System;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class ContainerTests
    {
        [Fact]
        public void ShouldListRegistrationsInToString()
        {
            // given
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>();
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithDefaultCtor { Status = "instance1" });
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithDefaultCtor { Status = "instance2" }, "two");
            container.RegisterInstanceAs<IInterface1>(new SimpleClassWithFailingToString(), "three");

            // when
            var result = container.ToString();
            Console.WriteLine(result);

            // then 
            result.Should().Contain("Reqnroll.BoDi.IObjectContainer -> <self>");
            result.Should().Contain("Reqnroll.RuntimeTests.BoDi.IInterface1 -> Instance: SimpleClassWithDefaultCtor: instance1");
            result.Should().Contain("Reqnroll.RuntimeTests.BoDi.IInterface1('one') -> Type: Reqnroll.RuntimeTests.BoDi.VerySimpleClass");
            result.Should().Contain("Reqnroll.RuntimeTests.BoDi.IInterface1('two') -> Instance: SimpleClassWithDefaultCtor: instance2");
            result.Should().Contain("Reqnroll.RuntimeTests.BoDi.IInterface1('three') -> Instance: simulated error");
        }
    }
}
