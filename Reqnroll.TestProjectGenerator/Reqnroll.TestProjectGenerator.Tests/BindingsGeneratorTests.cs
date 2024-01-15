using FluentAssertions;
using Reqnroll.TestProjectGenerator.Factories.BindingsGenerator;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Reqnroll.TestProjectGenerator.Tests
{
    public class BindingsGeneratorTests
    {
        private readonly BindingsGeneratorFactory _bindingsGeneratorFactory = new BindingsGeneratorFactory();

        [Theory(DisplayName = "BindingsGenerator should generate step definitions")]
        [InlineData(ProgrammingLanguage.CSharp, @"[Given(""Some method"")]public void SomeMethod(){}")]
        [InlineData(ProgrammingLanguage.FSharp, @"let [<Given>] `Some method` () = ()")]
        [InlineData(ProgrammingLanguage.VB, "<Given(\"Some method\")> _\r\n    Public Sub SomeMethod\\(\\)\r\n    End Sub")]
        public void BindingsGenerator_ShouldGenerateStepDefinition(
            ProgrammingLanguage targetLanguage,
            string stepDefinition)
        {
            // ARRANGE
            var generator = _bindingsGeneratorFactory.FromLanguage(targetLanguage);

            // ACT
            var bindingsFile = generator.GenerateStepDefinition(stepDefinition);

            // ASSERT
            bindingsFile.Content.Should().Contain(stepDefinition);
        }

        [Theory(DisplayName = "BindingsGenerator's result have 'Compile' as build action")]
        [InlineData(ProgrammingLanguage.CSharp, @"[Given(""Some method"")]public void SomeMethod(){}")]
        [InlineData(ProgrammingLanguage.FSharp, @"let [<Given>] `Some method` () = ()")]
        [InlineData(ProgrammingLanguage.VB, "<Given(\"Some method\")> _\r\n    Public Sub SomeMethod\\(\\)\r\n    End Sub")]
        public void BindingsGenerator_Result_ShouldHaveCompileAction(
            ProgrammingLanguage targetLanguage,
            string stepDefinition)
        {
            // ARRANGE
            var generator = _bindingsGeneratorFactory.FromLanguage(targetLanguage);

            // ACT
            var bindingsFile = generator.GenerateStepDefinition(stepDefinition);

            // ASSERT
            bindingsFile.BuildAction.Should().Be("Compile");
        }
    }
}
