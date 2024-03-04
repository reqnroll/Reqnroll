using System.Collections.Generic;
using Reqnroll.Assist;
using System.Linq;
using FluentAssertions;
using Reqnroll.Specs.Drivers.Parser;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class GherkinParserSteps
    {
        private readonly ParserDriver _parserDriver;
        private readonly TableHelpers _tableHelpers;

        public GherkinParserSteps(ParserDriver parserDriver, TableHelpers tableHelpers)
        {
            _parserDriver = parserDriver;
            _tableHelpers = tableHelpers;
        }

        [Given(@"there is a Gherkin file as")]
        public void GivenThereIsAGherkinFileAs(string text)
        {
            _parserDriver.FileContent = text;
        }

        [When(@"the file is parsed")]
        public void WhenTheFileIsParsed()
        {
            _parserDriver.ParseFile();
        }

        [Then(@"no parsing error is reported")]
        public void ThenNoParsingErrorIsReported()
        {
            _parserDriver.ParsingErrors.Should().BeEmpty("There are parsing errors");
        }

        [StepArgumentTransformation]
        public List<ExpectedError> ConvertExpectedErrors(Table table)
        {
            return _tableHelpers.CreateSet<ExpectedError>(table).ToList();
        }

        [Then(@"the following errors are provided")]
        public void ThenTheTheFollowingErrorsAreProvided(List<ExpectedError> expectedErrors)
        {
            _parserDriver.AssertErrors(expectedErrors);
        }
    }
}
