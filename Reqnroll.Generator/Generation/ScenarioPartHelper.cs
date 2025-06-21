using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin.Ast;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Parser;

namespace Reqnroll.Generator.Generation
{
    public class ScenarioPartHelper
    {
        private readonly ReqnrollConfiguration _reqnrollConfiguration;
        private readonly CodeDomHelper _codeDomHelper;
        private int _tableCounter;

        public ScenarioPartHelper(ReqnrollConfiguration reqnrollConfiguration, CodeDomHelper codeDomHelper)
        {
            _reqnrollConfiguration = reqnrollConfiguration;
            _codeDomHelper = codeDomHelper;
        }

        public void SetupFeatureBackground(TestClassGenerationContext generationContext)
        {
            if (!generationContext.Feature.HasFeatureBackground())
            {
                return;
            }

            var background = generationContext.Feature.Background;

            var backgroundMethod = generationContext.FeatureBackgroundMethod;

            backgroundMethod.Attributes = MemberAttributes.Public;
            backgroundMethod.Name = GeneratorConstants.BACKGROUND_NAME;

            _codeDomHelper.MarkCodeMemberMethodAsAsync(backgroundMethod);

            var statements = new List<CodeStatement>();
            using (new SourceLineScope(_reqnrollConfiguration, _codeDomHelper, statements, generationContext.Document.SourceFilePath, background.Location))
            {
            }

            foreach (var step in background.Steps)
            {
                GenerateStep(generationContext, statements, step, null);
            }
            backgroundMethod.Statements.AddRange(statements.ToArray());

        }
        #region Rule Background Support

        public void GenerateRuleBackgroundStepsApplicableForThisScenario(TestClassGenerationContext generationContext, ScenarioDefinitionInFeatureFile scenarioDefinition, List<CodeStatement> statementsWhenScenarioIsExecuted)
        {
            if (scenarioDefinition.Rule != null)
            {
                var rule = scenarioDefinition.Rule;
                IEnumerable<CodeStatement> ruleBackgroundStatements = GenerateBackgroundStatementsForRule(generationContext, rule);
                statementsWhenScenarioIsExecuted.AddRange(ruleBackgroundStatements);
            }
        }

        private IEnumerable<CodeStatement> GenerateBackgroundStatementsForRule(TestClassGenerationContext context, Rule rule)
        {
            var background = rule.Children.OfType<Background>().FirstOrDefault();

            if (background == null) return new List<CodeStatement>();

            var statements = new List<CodeStatement>();
            foreach (var step in background.Steps)
            {
                GenerateStep(context, statements, step, null);
            }

            return statements;
        }

        #endregion

        public void GenerateStep(TestClassGenerationContext generationContext, List<CodeStatement> statements, Step gherkinStep, ParameterSubstitution paramToIdentifier)
        {
            var testRunnerField = GetTestRunnerExpression();
            var scenarioStep = AsReqnrollStep(gherkinStep);

            //testRunner.Given("something");
            var arguments = new List<CodeExpression>
            {
                GetSubstitutedString(scenarioStep.Text, paramToIdentifier),
                GetDocStringArgExpression(scenarioStep.Argument as DocString, paramToIdentifier),
                GetTableArgExpression(scenarioStep.Argument as Gherkin.Ast.DataTable, statements, paramToIdentifier),
                new CodePrimitiveExpression(scenarioStep.Keyword)
            };

            using (new SourceLineScope(_reqnrollConfiguration, _codeDomHelper, statements, generationContext.Document.SourceFilePath, gherkinStep.Location))
            {
                string stepKeyWord = scenarioStep.StepKeyword switch
                {
                    StepKeyword.Given => "Given",
                    StepKeyword.When => "When",
                    StepKeyword.Then => "Then",
                    StepKeyword.And => "And",
                    StepKeyword.But => "But",
                    _ => throw new NotImplementedException(),
                };
                var expression = new CodeMethodInvokeExpression(
                    testRunnerField,
                    stepKeyWord + "Async",
                    arguments.ToArray());

                _codeDomHelper.MarkCodeMethodInvokeExpressionAsAwait(expression);

                statements.Add(new CodeExpressionStatement(expression));
            }
        }

        public CodeExpression GetStringArrayExpression(IEnumerable<Tag> tags)
        {
            if (!tags.Any())
            {
                return new CodeCastExpression(typeof(string[]), new CodePrimitiveExpression(null));
            }

            return new CodeArrayCreateExpression(typeof(string[]), tags.Select(tag => new CodePrimitiveExpression(tag.GetNameWithoutAt())).Cast<CodeExpression>().ToArray());
        }

        private ReqnrollStep AsReqnrollStep(Step step)
        {
            var reqnrollStep = step as ReqnrollStep;
            if (reqnrollStep == null)
            {
                throw new TestGeneratorException("The step must be a ReqnrollStep.");
            }

            return reqnrollStep;
        }

        private CodeExpression GetTableArgExpression(Gherkin.Ast.DataTable tableArg, List<CodeStatement> statements, ParameterSubstitution paramToIdentifier)
        {
            if (tableArg == null)
            {
                return new CodeCastExpression(new CodeTypeReference(typeof(Table), CodeTypeReferenceOptions.GlobalReference), new CodePrimitiveExpression(null));
            }

            _tableCounter++;

            //TODO[Gherkin3]: remove dependency on having the first row as header
            var header = tableArg.Rows.First();
            var body = tableArg.Rows.Skip(1).ToArray();

            //Table table0 = new Table(header...);
            var tableVar = new CodeVariableReferenceExpression("table" + _tableCounter);
            statements.Add(
                new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Table), CodeTypeReferenceOptions.GlobalReference), tableVar.VariableName,
                    new CodeObjectCreateExpression(
                        new CodeTypeReference(typeof(Table), CodeTypeReferenceOptions.GlobalReference),
                        GetStringArrayExpression(header.Cells.Select(c => c.Value), paramToIdentifier))));

            foreach (var row in body)
            {
                //table0.AddRow(cells...);
                statements.Add(new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        tableVar,
                        "AddRow",
                        GetStringArrayExpression(row.Cells.Select(c => c.Value), paramToIdentifier))));
            }

            return tableVar;
        }

        private CodeExpression GetDocStringArgExpression(DocString docString, ParameterSubstitution paramToIdentifier)
        {
            return GetSubstitutedString(docString == null ? null : docString.Content, paramToIdentifier);
        }

        public CodeExpression GetTestRunnerExpression()
        {
            return new CodeVariableReferenceExpression(GeneratorConstants.TESTRUNNER_FIELD);
        }

        private CodeExpression GetStringArrayExpression(IEnumerable<string> items, ParameterSubstitution paramToIdentifier)
        {
            return new CodeArrayCreateExpression(typeof(string[]), items.Select(item => GetSubstitutedString(item, paramToIdentifier)).ToArray());
        }

        private CodeExpression GetSubstitutedString(string text, ParameterSubstitution paramToIdentifier)
        {
            if (text == null)
            {
                return new CodeCastExpression(typeof(string), new CodePrimitiveExpression(null));
            }

            if (paramToIdentifier == null)
            {
                return new CodePrimitiveExpression(text);
            }

            var paramRe = new Regex(@"\<(?<param>[^\<\>]+)\>");
            var formatText = text.Replace("{", "{{").Replace("}", "}}");
            var arguments = new List<string>();

            formatText = paramRe.Replace(formatText, match =>
            {
                var param = match.Groups["param"].Value;
                string id;
                if (!paramToIdentifier.TryGetIdentifier(param, out id))
                {
                    return match.Value;
                }

                var argIndex = arguments.IndexOf(id);
                if (argIndex < 0)
                {
                    argIndex = arguments.Count;
                    arguments.Add(id);
                }

                return "{" + argIndex + "}";
            });

            if (arguments.Count == 0)
            {
                return new CodePrimitiveExpression(text);
            }

            var formatArguments = new List<CodeExpression> { new CodePrimitiveExpression(formatText) };
            formatArguments.AddRange(arguments.Select(id => new CodeVariableReferenceExpression(id)));

            return new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(string)),
                "Format",
                formatArguments.ToArray());
        }
        public void AddVariableForPickleIndex(CodeMemberMethod testMethod, bool pickleIdIncludedInParameters, int? pickleIndex)
        {
            if (!pickleIdIncludedInParameters && pickleIndex == null)
                throw new ArgumentNullException(nameof(pickleIndex));

            // string pickleId = "<pickleJar.CurrentPickleId>"; or
            // string pickleId = __pickleId;
            var pickleIdVariable = new CodeVariableDeclarationStatement(typeof(string), GeneratorConstants.PICKLEINDEX_VARIABLE_NAME,
                pickleIdIncludedInParameters ?
                    new CodeVariableReferenceExpression(GeneratorConstants.PICKLEINDEX_PARAMETER_NAME) :
                    new CodePrimitiveExpression(pickleIndex.Value.ToString()));
            testMethod.Statements.Add(pickleIdVariable);
        }
    }
}