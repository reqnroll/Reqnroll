using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Gherkin;
using Gherkin.Ast;
using Reqnroll.Parser.SemanticValidators;

namespace Reqnroll.Parser
{
    public class ReqnrollGherkinParser : IGherkinParser
    {
        private readonly IGherkinDialectProvider dialectProvider;
        private readonly List<ISemanticValidator> semanticValidators;

        public IGherkinDialectProvider DialectProvider
        {
            get { return dialectProvider; }
        }

        public ReqnrollGherkinParser(IGherkinDialectProvider dialectProvider)
        {
            this.dialectProvider = dialectProvider;
        }

        public ReqnrollGherkinParser(CultureInfo defaultLanguage)
            : this(new ReqnrollGherkinDialectProvider(defaultLanguage.Name))
        {
            semanticValidators = new List<ISemanticValidator>
            {
                new DuplicateScenariosValidator(),
                new DuplicateExamplesValidator(),
                new MissingExamplesValidator(),
                new DuplicateExamplesColumnHeadersValidator()
            };
        }

        private static StepKeyword GetStepKeyword(GherkinDialect dialect, string stepKeyword)
        {
            if (dialect.AndStepKeywords.Contains(stepKeyword)) // we need to check "And" first, as the '*' is also part of the Given, When and Then keywords
                return StepKeyword.And;
            if (dialect.GivenStepKeywords.Contains(stepKeyword))
                return StepKeyword.Given;
            if (dialect.WhenStepKeywords.Contains(stepKeyword))
                return StepKeyword.When;
            if (dialect.ThenStepKeywords.Contains(stepKeyword))
                return StepKeyword.Then;
            if (dialect.ButStepKeywords.Contains(stepKeyword))
                return StepKeyword.But;

            return StepKeyword.And;
        }

        private class ReqnrollAstBuilder : AstBuilder<ReqnrollDocument>
        {
            private readonly ReqnrollDocumentLocation documentLocation;
            private ScenarioBlock scenarioBlock = ScenarioBlock.Given;

            public ReqnrollAstBuilder(ReqnrollDocumentLocation documentLocation)
            {
                this.documentLocation = documentLocation;
            }

            protected override Feature CreateFeature(Tag[] tags, Location location, string language, string keyword, string name, string description, IHasLocation[] children, AstNode node)
            {
                return new ReqnrollFeature(tags, location, language, keyword, name, description, children);
            }

            protected override Scenario CreateScenario(Tag[] tags, Location location, string keyword, string name, string description, Step[] steps, Examples[] examples, AstNode node)
            {
                ResetBlock();

                if (examples != null && examples.Length > 0)
                {
                    return new ScenarioOutline(tags, location, keyword, name, description, steps, examples);
                }

                return base.CreateScenario(tags, location, keyword, name, description, steps, examples, node);

            }

            protected override Step CreateStep(Location location, string keyword, StepKeywordType keywordType, string text, StepArgument argument, AstNode node)
            {
                var token = node.GetToken(TokenType.StepLine);
                var stepKeyword = GetStepKeyword(token.MatchedGherkinDialect, keyword);
                scenarioBlock = stepKeyword.ToScenarioBlock() ?? scenarioBlock;

                return new ReqnrollStep(location, keyword, keywordType, text, argument, stepKeyword, scenarioBlock);
            }

            private void ResetBlock()
            {
                scenarioBlock = ScenarioBlock.Given;
            }

            protected override GherkinDocument CreateGherkinDocument(Feature feature, Comment[] gherkinDocumentComments, AstNode node)
            {
                return new ReqnrollDocument((ReqnrollFeature)feature, gherkinDocumentComments, documentLocation);
            }

            protected override Background CreateBackground(Location location, string keyword, string name, string description, Step[] steps, AstNode node)
            {
                ResetBlock();
                return base.CreateBackground(location, keyword, name, description, steps, node);
            }
        }

        public ReqnrollDocument Parse(TextReader featureFileReader, ReqnrollDocumentLocation documentLocation)
        {
            var parser = new Parser<ReqnrollDocument>(CreateAstBuilder(documentLocation));
            ReqnrollDocument reqnrollDocument = parser.Parse(CreateTokenScanner(featureFileReader), CreateTokenMatcher());

            CheckSemanticErrors(reqnrollDocument);

            return reqnrollDocument;
        }

        protected virtual ITokenScanner CreateTokenScanner(TextReader featureFileReader)
        {
            return new TokenScanner(featureFileReader);
        }

        protected virtual ITokenMatcher CreateTokenMatcher()
        {
            return new TokenMatcher(dialectProvider);
        }

        protected virtual IAstBuilder<ReqnrollDocument> CreateAstBuilder(ReqnrollDocumentLocation documentLocation)
        {
            return new ReqnrollAstBuilder(documentLocation);
        }

        protected virtual void CheckSemanticErrors(ReqnrollDocument reqnrollDocument)
        {
            if (reqnrollDocument?.ReqnrollFeature == null)
                return;

            var errors = semanticValidators
                .SelectMany(x => x.Validate(reqnrollDocument.ReqnrollFeature))
                .ToList();

            // collect
            if (errors.Count == 1)
                throw errors[0];
            if (errors.Count > 1)
                throw new CompositeParserException(errors.ToArray());
        }
    }
}