using System;
using System.Collections.Generic;
using FluentAssertions;
using Reqnroll.BoDi;
using Xunit;

namespace Reqnroll.RuntimeTests.BoDi
{
    public class NamedRegistrationTests
    {
        [Fact]
        public void ShouldBeAbleToRegisterTypeWithName()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("a_name");
        }

        [Fact]
        public void ShouldBeAbleToRegisterTypeWithNameDynamically()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<IInterface1>(typeof(VerySimpleClass), "a_name");
        }

        [Fact]
        public void NamedRegistrationShouldNotInflucenceNormalRegistrations()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("a_name");

            // when
            Action act = () => container.Resolve<IInterface1>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldBeAbleToResolveWithName()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("a_name");

            // when
            var obj = container.Resolve<IInterface1>("a_name");

            // then
            obj.Should().BeOfType<VerySimpleClass>();
        }

        [Fact]
        public void ShouldNotReuseObjectsWithTheSameTypeButResolvedWithDifferentName()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("a_name");
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("another_name");

            // when
            var obj = container.Resolve<IInterface1>("a_name");
            var otherObj = container.Resolve<IInterface1>("another_name");

            // then
            obj.Should().NotBeSameAs(otherObj);
        }

        [Fact]
        public void ShouldBeAbleToRegisterMultipleTypesWithDifferentNames()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>("two");

            // when
            var oneObj = container.Resolve<IInterface1>("one");
            var twoObj = container.Resolve<IInterface1>("two");

            // then
            oneObj.Should().NotBeSameAs(twoObj);
            oneObj.Should().BeOfType<VerySimpleClass>();
            twoObj.Should().BeOfType<SimpleClassWithDefaultCtor>();
        }

        [Fact]
        public void ShouldBeAbleToResolveNamedInstancesAsDictionary()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>("two");

            // when
            var instanceDict = container.Resolve<IDictionary<string, IInterface1>>();

            // then
            instanceDict.Keys.Should().Contain("one");
            instanceDict.Keys.Should().Contain("two");
            instanceDict["one"].Should().BeOfType<VerySimpleClass>();
            instanceDict["two"].Should().BeOfType<SimpleClassWithDefaultCtor>();
        }

        [Fact]
        public void ShouldBeAbleToResolveNamedInstancesAsDictionaryEvenIfThereWasNoRegistrations()
        {
            var container = new ObjectContainer();

            // when
            var instanceDict = container.Resolve<IDictionary<string, IInterface1>>();

            // then
            instanceDict.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldBeAbleToResolveNamedInstancesAsEnumKeyDictionary()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");
            container.RegisterTypeAs<SimpleClassWithDefaultCtor, IInterface1>("two");

            // when
            var instanceDict = container.Resolve<IDictionary<MyEnumKey, IInterface1>>();

            // then
            instanceDict.Keys.Should().Contain(MyEnumKey.One);
            instanceDict.Keys.Should().Contain(MyEnumKey.Two);
            instanceDict[MyEnumKey.One].Should().BeOfType<VerySimpleClass>();
            instanceDict[MyEnumKey.Two].Should().BeOfType<SimpleClassWithDefaultCtor>();
        }

        [Fact]
        public void ShouldNotBeAbleToResolveNamedInstancesDictionaryOtherThanStringAndEnumKey()
        {
            var container = new ObjectContainer();

            // when
            Action act = () => container.Resolve<IDictionary<int, IInterface1>>();
            act.Should().ThrowExactly<ObjectContainerException>();
        }

        [Fact]
        public void ShouldBeAbleToInjectResolvedName()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<SimpleClassWithRegisteredNameDependency, IInterface1>("a_name");

            // when
            var obj = container.Resolve<IInterface1>("a_name");

            // then
            obj.Should().BeOfType<SimpleClassWithRegisteredNameDependency>();
            ((SimpleClassWithRegisteredNameDependency)obj).RegisteredName.Should().Be("a_name");
        }

        [Fact]
        public void ShouldBeAbleToResolveNamedInstancesWhenDependingOnOther()
        {
            var container = new ObjectContainer();
            container.RegisterTypeAs<Interface1DependingOnAnotherImplementation, IInterface1>("two");
            container.RegisterTypeAs<VerySimpleClass, IInterface1>("one");

            // when
            container.Resolve<IDictionary<string, IInterface1>>();

            // then
            //should not throw Collection was modified; error
        }
    }
}
