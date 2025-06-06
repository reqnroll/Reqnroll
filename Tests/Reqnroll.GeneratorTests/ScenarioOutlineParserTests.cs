using FluentAssertions;
using Xunit;
using System;
using System.Globalization;
using System.IO;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests
{
    public class ScenarioOutlineParserTests
    {
        [Fact]
        public void Parser_doesnt_throw_exception_when_Examples_are_missing_in_Scenario_Outline()
        {
            // this is accepted by Gherkin v6 and treated as Scenario
            var feature = @"Feature: Missing
                            Scenario Outline: No Examples";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().NotThrow();
        }

        [Fact]
        public void Parser_throws_meaningful_exception_when_Examples_are_empty_in_Scenario_Outline()
        {
            var feature = @"Feature: Missing
                            Scenario Outline: No Examples
                            Given something
                            
                            Examples:";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().Throw<SemanticParserException>().WithMessage("(2:29): Scenario Outline 'No Examples' has no examples defined")
                .And.Location?.Line.Should().Be(2);
        }

        [Fact]
        public void Parser_throws_meaningful_exception_when_Examples_have_header_but_are_empty_in_Scenario_Outline()
        {
            var feature = @"Feature: Missing
                            Scenario Outline: No Examples
                            Given something
                            
                            Examples:
                            | Column |
                            ";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().Throw<SemanticParserException>().WithMessage("(2:29): Scenario Outline 'No Examples' has no examples defined")
                .And.Location?.Line.Should().Be(2);
        }

        [Fact]
        public void Parser_doesnt_throw_exception_when_Examples_are_missing_in_multiple_Scenario_Outlines()
        {
            // these are accepted by Gherkin v6 and treated as Scenarios
            var feature = @"Feature: Missing
                            Scenario Outline: No Examples
                            Scenario Outline: Still no Examples";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().NotThrow();
        }

        [Fact]
        public void Parser_doesnt_throw_exception_when_Examples_are_provided_for_Scenario_Outline()
        {
            var feature = @"Feature: Missing
                    Scenario Outline: No Examples
                    Given I do <thing>
                    Examples:
                    | thing |
                    | test  |";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().NotThrow();
        }

        [Fact]
        public void Parser_throws_meaningful_exception_when_Examples_have_duplicate_header_in_Scenario_Outline()
        {
            var feature = @"Feature: Duplicate
                            Scenario Outline: Duplicate Examples table headers
                            Given I am <acting>
                            
                            Examples:
                            | acting  | acting   |
                            | driving | drinking |
                            ";

            var parser = new ReqnrollGherkinParser(CultureInfo.GetCultureInfo("en"));

            Action act = () => parser.Parse(new StringReader(feature), null);

            act.Should().Throw<SemanticParserException>().WithMessage("(2:29): Scenario Outline 'Duplicate Examples table headers' already contains an example column with header 'acting'")
               .And.Location?.Line.Should().Be(2);
        }
    }
}
