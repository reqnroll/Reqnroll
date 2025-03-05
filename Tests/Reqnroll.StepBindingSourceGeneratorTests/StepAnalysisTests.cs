using Microsoft.CodeAnalysis;

namespace Reqnroll.StepBindingSourceGenerator;

public class StepAnalysisTests
{
    [Fact]
    public void StepBindingOnMethodWithReturnValue_GeneratesErrorStepMethodMustReturnVoidOrTask()
    {
        var source = MarkedSourceText.Parse(
            """
            using Reqnroll;
            
            namespace Sample.Tests;
            
            [Binding]
            public class GameSteps
            {
                [When]
                public string ⇥WhenMakerStartsAGame⇤()
                {
                    return "Game started";
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepMethodMustReturnVoidOrTask,
                    source.GetMarkedLocation("/spec/Sample.feature"),
                    "GameSteps.WhenmakerStartsAGame",
                    "void")
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void StepBindingOnAsyncMethodWithVoidReturn_GeneratesErrorAsyncStepMethodMustReturnTask()
    {
        var source = MarkedSourceText.Parse(
            """
            using System.Threading.Tasks;
            using Reqnroll;
            
            namespace Sample.Tests;
            
            [Binding]
            public class GameSteps
            {
                [When]
                public async void ⇥WhenMakerStartsAGame⇤()
                {
                    await Task.Delay(100);
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorAsyncStepMethodMustReturnTask,
                    source.GetMarkedLocation("/spec/Sample.feature"),
                    "GameSteps.WhenMakerStartsAGame",
                    "async")
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void StepBindingWithNullText_GeneratesErrorStepTextCannotBeEmpty()
    {
        var source = MarkedSourceText.Parse(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When(⇥null⇤)]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void StepBindingWithDefaultText_GeneratesErrorStepTextCannotBeEmpty()
    {
        var source = MarkedSourceText.Parse(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When(⇥default⇤)]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void StepBindingWithEmptyText_GeneratesErrorStepTextCannotBeEmpty()
    {
        var source = MarkedSourceText.Parse(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When(⇥""⇤)]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public void StepBindingWithWhitespaceText_GeneratesErrorStepTextCannotBeEmpty(string whitespace)
    {
        var source = MarkedSourceText.Parse(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When(⇥"{{whitespace}}"⇤)]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }


    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public void StepBindingWithLeadingWhitespace_GeneratesWarningStepTextHasLeadingWhitespace(string whitespace)
    {
        var source = MarkedSourceText.Parse(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When("⇥{{whitespace}}⇤Maker starts a game")]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.WarningStepTextHasLeadingWhitespace,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public void StepBindingWithTrailingWhitespace_GeneratesWarningStepTextHasTrailingWhitespace(string whitespace)
    {
        var source = MarkedSourceText.Parse(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When("Maker starts a game⇥{{whitespace}}⇤")]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.WarningStepTextHasTrailingWhitespace,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }
}
