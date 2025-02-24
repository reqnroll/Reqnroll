using System.CodeDom;
using System.Collections.Generic;
using Reqnroll.BoDi;
using NSubstitute;
using Xunit;
using Reqnroll.Generator;
using Reqnroll.Generator.UnitTestConverter;
using FluentAssertions;
using Gherkin.Ast;

namespace Reqnroll.GeneratorTests
{
    
    public class DecoratorRegistryTests
    {
        private IObjectContainer container;
        
        public DecoratorRegistryTests()
        {
            container = new ObjectContainer();
        }

        internal static ITestClassTagDecorator CreateTestClassTagDecoratorMock(string expectedTag = null)
        {
            var testClassDecoratorMock = Substitute.For<ITestClassTagDecorator>();
            testClassDecoratorMock.ApplyOtherDecoratorsForProcessedTags.Returns(false);
            testClassDecoratorMock.RemoveProcessedTags.Returns(true);
            testClassDecoratorMock.Priority.Returns(PriorityValues.Normal);
            if (expectedTag == null)
                testClassDecoratorMock.CanDecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>()).Returns(true);
            else
                testClassDecoratorMock.CanDecorateFrom(expectedTag, Arg.Any<TestClassGenerationContext>()).Returns(true);
            return testClassDecoratorMock;
        }

        internal static ITestClassDecorator CreateTestClassDecoratorMock()
        {
            var testClassDecoratorMock = Substitute.For<ITestClassDecorator>();
            testClassDecoratorMock.Priority.Returns(PriorityValues.Normal);
            testClassDecoratorMock.CanDecorateFrom(Arg.Any<TestClassGenerationContext>()).Returns(true);
            return testClassDecoratorMock;
        }

        internal static ITestMethodDecorator CreateTestMethodDecoratorMock()
        {
            var testClassDecoratorMock = Substitute.For<ITestMethodDecorator>();
            testClassDecoratorMock.Priority.Returns(PriorityValues.Normal);
            testClassDecoratorMock.CanDecorateFrom(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>()).Returns(true);
            return testClassDecoratorMock;
        }

        internal static ITestMethodTagDecorator CreateTestMethodTagDecoratorMock(string expectedTag = null)
        {
            var testClassDecoratorMock = Substitute.For<ITestMethodTagDecorator>();
            testClassDecoratorMock.ApplyOtherDecoratorsForProcessedTags.Returns(false);
            testClassDecoratorMock.RemoveProcessedTags.Returns(true);
            testClassDecoratorMock.Priority.Returns(PriorityValues.Normal);
            if (expectedTag == null)
                testClassDecoratorMock.CanDecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>()).Returns(true);
            else
                testClassDecoratorMock.CanDecorateFrom(expectedTag, Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>()).Returns(true);
            return testClassDecoratorMock;
        }

        private DecoratorRegistry CreateDecoratorRegistry()
        {
            return new DecoratorRegistry(container);
        }

        private static TestClassGenerationContext CreateGenerationContext(string tag)
        {
            return new TestClassGenerationContext(null, ParserHelper.CreateAnyDocument(new []{ tag }), null, null, null, null, null, null, null, null, null, null, null, true);
        }

        [Fact]
        public void Should_decorate_test_class()
        {
            var testClassDecoratorMock = CreateTestClassDecoratorMock();
            container.RegisterInstanceAs(testClassDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestClass(CreateGenerationContext("dummy"), out unprocessedTags);

            testClassDecoratorMock.Received().DecorateFrom(Arg.Any<TestClassGenerationContext>());
        }

        [Fact]
        public void Should_decorate_test_class_when_not_applicable()
        {
            var testClassDecoratorMock = CreateTestClassDecoratorMock();
            testClassDecoratorMock.CanDecorateFrom(Arg.Any<TestClassGenerationContext>()).Returns(false);
            container.RegisterInstanceAs(testClassDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestClass(CreateGenerationContext("dummy"), out unprocessedTags);

            testClassDecoratorMock.DidNotReceive().DecorateFrom(Arg.Any<TestClassGenerationContext>());
        }

        [Fact]
        public void Should_decorate_test_class_based_on_tag()
        {
            var testClassDecoratorMock = CreateTestClassTagDecoratorMock();
            container.RegisterInstanceAs(testClassDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestClass(CreateGenerationContext("foo"), out unprocessedTags);

            testClassDecoratorMock.Received().DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>());
        }

        [Fact]
        public void Should_remove_processed_tag_from_test_class_category_list()
        {
            var testClassDecoratorMock = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock.RemoveProcessedTags.Returns(true);
            container.RegisterInstanceAs(testClassDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> classCats = null;
            registry.DecorateTestClass(CreateGenerationContext("foo"), out classCats);

            classCats.Should().NotBeNull();
            classCats.Should().NotContain("foo");
        }

        [Fact]
        public void Should_keep_processed_tag_from_test_class_category_list()
        {
            var testClassDecoratorMock = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock.RemoveProcessedTags.Returns(false);
            container.RegisterInstanceAs(testClassDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> classCats = null;
            registry.DecorateTestClass(CreateGenerationContext("foo"), out classCats);

            classCats.Should().NotBeNull();
            classCats.Should().Contain("foo");
        }

        [Fact]
        public void Should_allow_multiple_decorators()
        {
            var testClassDecoratorMock1 = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock1.ApplyOtherDecoratorsForProcessedTags.Returns(true);
            container.RegisterInstanceAs(testClassDecoratorMock1, "foo1");

            var testClassDecoratorMock2 = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock2.ApplyOtherDecoratorsForProcessedTags.Returns(true);
            container.RegisterInstanceAs(testClassDecoratorMock2, "foo2");

            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestClass(CreateGenerationContext("foo"), out unprocessedTags);

            testClassDecoratorMock1.Received().DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>());
            testClassDecoratorMock2.Received().DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>());
        }

        [Fact]
        public void Should_higher_priority_decorator_applied_first()
        {
            List<string> executionOrder = new List<string>();

            var testClassDecoratorMock1 = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock1.ApplyOtherDecoratorsForProcessedTags.Returns(true);
            testClassDecoratorMock1
                .When(m => m.DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>()))
                .Do((_) => executionOrder.Add("foo1"));

            container.RegisterInstanceAs(testClassDecoratorMock1, "foo1");

            var testClassDecoratorMock2 = CreateTestClassTagDecoratorMock();
            testClassDecoratorMock2.ApplyOtherDecoratorsForProcessedTags.Returns(true);
            testClassDecoratorMock2.Priority.Returns(PriorityValues.High);
            testClassDecoratorMock2
                .When(m => m.DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>()))
                .Do((_) =>executionOrder.Add("foo2"));
            
            container.RegisterInstanceAs(testClassDecoratorMock2, "foo2");

            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestClass(CreateGenerationContext("foo"), out unprocessedTags);

            executionOrder.Should().Equal(new[] { "foo2", "foo1" });
        }

        [Fact]
        public void Should_not_decorate_test_method_for_feature_tag()
        {
            var testMethodDecoratorMock = CreateTestMethodTagDecoratorMock();
            container.RegisterInstanceAs(testMethodDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestMethod(CreateGenerationContext("foo"), null, new Tag[] { }, out unprocessedTags);

            testMethodDecoratorMock.DidNotReceive().DecorateFrom("foo", Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>());
        }

        [Fact]
        public void Should_decorate_test_method_for_scenario_tag()
        {
            var testMethodDecoratorMock = CreateTestMethodTagDecoratorMock();
            container.RegisterInstanceAs(testMethodDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestMethod(CreateGenerationContext("dummy"), null, ParserHelper.GetTags("foo"), out unprocessedTags);

            testMethodDecoratorMock.Received().DecorateFrom(Arg.Any<string>(), Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>());
        }

        [Fact]
        public void Should_decorate_test_method()
        {
            var testMethodDecoratorMock = CreateTestMethodDecoratorMock();
            container.RegisterInstanceAs(testMethodDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestMethod(CreateGenerationContext("dummy"), null, ParserHelper.GetTags("dummy"), out unprocessedTags);

            testMethodDecoratorMock.Received().DecorateFrom(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>());
        }

        [Fact]
        public void Should_not_decorate_test_method_when_not_applicable()
        {
            var testMethodDecoratorMock = CreateTestMethodDecoratorMock();
            testMethodDecoratorMock.CanDecorateFrom(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>())
                .Returns(false);
            container.RegisterInstanceAs(testMethodDecoratorMock, "foo");
            var registry = CreateDecoratorRegistry();

            List<string> unprocessedTags;
            registry.DecorateTestMethod(CreateGenerationContext("dummy"), null, ParserHelper.GetTags("dummy"), out unprocessedTags);

            testMethodDecoratorMock.DidNotReceive().DecorateFrom(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>());
        }
    }

    internal class DecoratorRegistryStub : IDecoratorRegistry
    {
        public void DecorateTestClass(TestClassGenerationContext generationContext, out List<string> unprocessedTags)
        {
            unprocessedTags = new List<string>();
        }

        public void DecorateTestMethod(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<Tag> tags, out List<string> unprocessedTags)
        {
            unprocessedTags = new List<string>();
        }
    }
}
