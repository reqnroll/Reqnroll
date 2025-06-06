using System.Collections.Generic;
using System.Linq;
using Gherkin;
using Gherkin.Ast;
using Reqnroll.ExternalData.ReqnrollPlugin.Transformation;
using Reqnroll.Parser;
using Xunit;

namespace Reqnroll.PluginTests.ExternalData
{
    public class ScenarioTransformationTests
    {
        class TestableScenarioTransformation : ScenarioTransformation
        {
            private readonly Dictionary<Scenario, Scenario> _transformations;

            public TestableScenarioTransformation(Scenario fromScenario, Scenario toScenario)
                : this(new Dictionary<Scenario, Scenario> { {fromScenario, toScenario } })
            { }
            
            public TestableScenarioTransformation(Dictionary<Scenario, Scenario> transformations)
            {
                _transformations = transformations;
            }

            protected override Scenario GetTransformedScenarioOutline(ScenarioOutline scenarioOutline) =>
                _transformations.ContainsKey(scenarioOutline) ? _transformations[scenarioOutline] : null;

            protected override Scenario GetTransformedScenario(Scenario scenario) =>
                _transformations.ContainsKey(scenario) ? _transformations[scenario] : null;
        }


        private const string DOCUMENT_PATH = @"C:\Temp\Sample.feature";
        private readonly Dictionary<Scenario, Scenario> _transformations = new();
        private int _scenarioCounter = 0;

        private TestableScenarioTransformation CreateSut() => new(_transformations);
        private TestableScenarioTransformation CreateSut(Scenario fromScenario, Scenario toScenario) => new(fromScenario, toScenario);

        private ReqnrollDocument CreateReqnrollDocument(params IHasLocation[] children)
        {
            return new(new ReqnrollFeature(new Tag[0], default, null, "Feature", "Sample feature", "", children ?? new IHasLocation[0]), new Comment[0], 
                       new ReqnrollDocumentLocation(DOCUMENT_PATH));
        }

        private ScenarioOutline CreateScenarioOutline()
        {
            return new(
                new Tag[0],
                default,
                "Scenario Outline",
                $"SO {++_scenarioCounter}",
                null,
                new[] { new Step(default, "Given ", StepKeywordType.Context, "the customer has <product>", null) },
                new[] { new Examples(new Tag[0], default, "Examples", "", "", new Gherkin.Ast.TableRow(default, new[] { new TableCell(default, "product") }), new Gherkin.Ast.TableRow[0]) });
        }

        private Scenario CreateScenario()
        {
            return new(
                new Tag[0],
                default,
                "Scenario",
                $"S {++_scenarioCounter}",
                null,
                new[] { new Step(default, "Given ", StepKeywordType.Context, "the customer has food", null) }, 
                null);
        }

        [Fact]
        public void Should_return_the_same_document_when_no_transformation()
        {
            var document = CreateReqnrollDocument(CreateScenarioOutline());

            var sut = CreateSut();

            var result = sut.TransformDocument(document);
            
            Assert.NotNull(result);
            Assert.Same(document, result);
        }

        [Fact]
        public void Should_transform_scenario()
        {
            var fromScenario = CreateScenario();
            var toScenario = CreateScenario();
            var document = CreateReqnrollDocument(fromScenario);

            var sut = CreateSut(fromScenario, toScenario);

            var result = sut.TransformDocument(document);
            
            var transformedScenario = result.Feature.Children.OfType<Scenario>().FirstOrDefault();
            Assert.NotNull(transformedScenario);
            Assert.Same(toScenario, transformedScenario);
        }

        [Fact]
        public void Should_transform_scenario_outline()
        {
            var fromScenarioOutline = CreateScenarioOutline();
            var toScenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocument(fromScenarioOutline);

            var sut = CreateSut(fromScenarioOutline, toScenarioOutline);

            var result = sut.TransformDocument(document);
            
            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.Same(toScenarioOutline, transformedOutline);
        }

        [Fact]
        public void Should_transform_scenario_to_outline()
        {
            var fromScenario = CreateScenario();
            var toScenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocument(fromScenario);

            var sut = CreateSut(fromScenario, toScenarioOutline);

            var result = sut.TransformDocument(document);
            
            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.Same(toScenarioOutline, transformedOutline);
        }

        [Fact]
        public void Should_transform_scenarios_within_a_rule()
        {
            var fromScenarioOutline = CreateScenarioOutline();
            var toScenarioOutline = CreateScenarioOutline();
            var rule = new Rule(null, default, "Rule", "My rule", null, new IHasLocation[] { fromScenarioOutline });
            var document = CreateReqnrollDocument(rule);

            var sut = CreateSut(fromScenarioOutline, toScenarioOutline);

            var result = sut.TransformDocument(document);
            
            var transformedOutline = result.Feature.Children.OfType<Rule>().SelectMany(r => r.Children)
                                           .OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.Same(toScenarioOutline, transformedOutline);
        }

        [Fact]
        public void Should_keep_the_rule_when_there_was_no_transformation_within()
        {
            var fromScenarioOutline = CreateScenarioOutline();
            var toScenarioOutline = CreateScenarioOutline();
            var rule = new Rule(null, default, "Rule", "My rule", null, new IHasLocation[] { CreateScenarioOutline() });
            var document = CreateReqnrollDocument(fromScenarioOutline, rule);

            var sut = CreateSut(fromScenarioOutline, toScenarioOutline);

            var result = sut.TransformDocument(document);
            
            var transformedRule = result.Feature.Children.OfType<Rule>().FirstOrDefault();
            Assert.NotNull(transformedRule);
            Assert.Same(rule, transformedRule);
        }

        [Fact]
        public void Should_keep_background()
        {
            var fromScenarioOutline = CreateScenarioOutline();
            var toScenarioOutline = CreateScenarioOutline();
            var background = new Background(default, "Background", null, null, new Step[0]);
            var document = CreateReqnrollDocument(background, fromScenarioOutline);

            var sut = CreateSut(fromScenarioOutline, toScenarioOutline);

            var result = sut.TransformDocument(document);
            
            var transformedBackground = result.Feature.Children.OfType<Background>().FirstOrDefault();
            Assert.NotNull(transformedBackground);
            Assert.Same(background, transformedBackground);
        }

        [Fact]
        public void Should_transform_scenarios_outside_and_within_a_rule()
        {
            var fromScenarioOutline = CreateScenarioOutline();
            var toScenarioOutline = CreateScenarioOutline();
            var fromScenarioOutlineInRule = CreateScenarioOutline();
            var toScenarioOutlineInRule = CreateScenarioOutline();
            _transformations.Add(fromScenarioOutline, toScenarioOutline);
            _transformations.Add(fromScenarioOutlineInRule, toScenarioOutlineInRule);
            var rule = new Rule(null, default, "Rule", "My rule", null, new IHasLocation[] { fromScenarioOutlineInRule });
            var document = CreateReqnrollDocument(fromScenarioOutline, rule);

            var sut = CreateSut();

            var result = sut.TransformDocument(document);

            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.Same(toScenarioOutline, transformedOutline);

            var transformedOutlineInRule = result.Feature.Children.OfType<Rule>().SelectMany(r => r.Children)
                                                 .OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutlineInRule);
            Assert.Same(toScenarioOutlineInRule, transformedOutlineInRule);
        }
    }
}
