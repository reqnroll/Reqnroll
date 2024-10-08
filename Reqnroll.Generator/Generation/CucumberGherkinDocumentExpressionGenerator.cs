using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Io.Cucumber.Messages.Types;
using Reqnroll.Generator.CodeDom;
using System;
using System.Linq;
using System.CodeDom;
using System.Collections.Generic;

namespace Reqnroll.Generator.Generation
{

    /// <summary>
    /// This class is responsible for generating a CodeDom expression that represents code that will recreate the given Cucumber Gherkin Document.
    /// </summary>
    internal class CucumberGherkinDocumentExpressionGenerator : CucumberMessage_TraversalVisitorBase
    {
        CodeDomHelper _codeDomHelper;

        Io.Cucumber.Messages.Types.GherkinDocument _gherkinDocument;
        CodeExpression _gherkinDocumentExpression;

        CodeExpression _feature;
        List<CodeExpression> _CommentsList;
        CodeExpression _location;
        List<CodeExpression> _TagsList;
        List<CodeExpression> _FeatureChildrenList;
        CodeExpression _background;
        CodeExpression _scenario;
        CodeExpression _rule;
        List<CodeExpression> _RuleChildrenList;
        List<CodeExpression> _StepsList;
        List<CodeExpression> _ExamplesList;
        CodeExpression _dataTable;
        CodeExpression _DocString;
        List<CodeExpression> _TableRowsList;
        List<CodeExpression> _TableCellsList;

        public CucumberGherkinDocumentExpressionGenerator(CodeDomHelper codeDomHelper)
        {
            _codeDomHelper = codeDomHelper;
        }
        private void Reset()
        {
            _feature = null;
            _CommentsList = new List<CodeExpression>();
            _location = null;
            _TagsList = new List<CodeExpression>();
            _FeatureChildrenList = new List<CodeExpression>();
            _background = null;
            _scenario = null;
            _rule = null;
            _RuleChildrenList = new List<CodeExpression>();
            _StepsList = new List<CodeExpression>();
            _ExamplesList = new List<CodeExpression>();
            _dataTable = null;
            _DocString = null;
            _TableRowsList = new List<CodeExpression>();
            _TableCellsList = new List<CodeExpression>();

        }

        public CodeExpression GenerateGherkinDocumentExpression(GherkinDocument gherkinDocument)
        {
            Reset();

            _gherkinDocument = gherkinDocument;
            _CommentsList = new List<CodeExpression>();

            Visit(gherkinDocument);

            var commentsListExpr = new CodeTypeReference(typeof(List<Comment>));
            var initializer = new CodeArrayCreateExpression(typeof(Comment), _CommentsList.ToArray());

            _gherkinDocumentExpression = new CodeObjectCreateExpression(typeof(GherkinDocument),
                new CodePrimitiveExpression(_gherkinDocument.Uri),
                _feature,
                new CodeObjectCreateExpression(commentsListExpr, initializer));

            return _gherkinDocumentExpression;
        }


        public override void Visit(Feature feature)
        {
            var location = _location;
            var featureChildren = _FeatureChildrenList;
            _FeatureChildrenList = new List<CodeExpression>();
            var tags = _TagsList;
            _TagsList = new List<CodeExpression>();

            base.Visit(feature);

            var tagsListExpr = new CodeTypeReference(typeof(List<Tag>));
            var tagsinitializer = new CodeArrayCreateExpression(typeof(Tag), _TagsList.ToArray());

            var FClistExpr = new CodeTypeReference(typeof(List<FeatureChild>));
            var initializer = new CodeArrayCreateExpression(typeof(FeatureChild), _FeatureChildrenList.ToArray());

            _feature = new CodeObjectCreateExpression(typeof(Feature),
                _location,
                new CodeObjectCreateExpression(tagsListExpr, tagsinitializer),
                new CodePrimitiveExpression(feature.Language),
                new CodePrimitiveExpression(feature.Keyword),
                new CodePrimitiveExpression(feature.Name),
                new CodePrimitiveExpression(feature.Description),
                new CodeObjectCreateExpression(FClistExpr, initializer));

            _location = location;
            _FeatureChildrenList = featureChildren;
            _TagsList = tags;
        }

        public override void Visit(Comment comment)
        {
            var location = _location;

            base.Visit(comment);

            _CommentsList.Add(new CodeObjectCreateExpression(typeof(Comment),
                _location,
                new CodePrimitiveExpression(comment.Text)));

            _location = location;
        }

        public override void Visit(Tag tag)
        {
            var location = _location;

            base.Visit(tag);

            _TagsList.Add(new CodeObjectCreateExpression(typeof(Tag),
                _location,
                new CodePrimitiveExpression(tag.Name),
                new CodePrimitiveExpression(tag.Id)));

            _location = location;

        }

        public override void Visit(Location location)
        {
            base.Visit(location);
            var columnExprTypeExpr = new CodeTypeReference(typeof(Nullable<>));
            columnExprTypeExpr.TypeArguments.Add(typeof(long));

            _location = new CodeObjectCreateExpression(typeof(Location),
                new CodePrimitiveExpression(location.Line),
                location.Column == null ? new CodeObjectCreateExpression(columnExprTypeExpr) :new CodeObjectCreateExpression(columnExprTypeExpr, new CodePrimitiveExpression(location.Column)));

        }

        public override void Visit(FeatureChild featureChild)
        {
            var rule = _rule;
            var scenario = _scenario;
            var background = _background;

            _rule = null;
            _scenario = null;
            _background = null;

            base.Visit(featureChild);

            _FeatureChildrenList.Add(new CodeObjectCreateExpression(typeof(FeatureChild),
                _rule ?? new CodePrimitiveExpression(null),
                _background ?? new CodePrimitiveExpression(null),
                _scenario ?? new CodePrimitiveExpression(null)));

            _rule = rule;
            _scenario = scenario;
            _background = background;
        }

        public override void Visit(Io.Cucumber.Messages.Types.Rule rule)
        {
            var location = _location;
            var ruleChildren = _RuleChildrenList;
            _RuleChildrenList = new List<CodeExpression>();
            var tags = _TagsList;
            _TagsList = new List<CodeExpression>();

            base.Visit(rule);

            var tagsListExpr = new CodeTypeReference(typeof(List<Tag>));
            var tagsinitializer = new CodeArrayCreateExpression(typeof(Tag), _TagsList.ToArray());

            var ruleChildrenListExpr = new CodeTypeReference(typeof(List<RuleChild>));
            var ruleChildrenInitializer = new CodeArrayCreateExpression(typeof(RuleChild), _RuleChildrenList.ToArray());

            _rule = new CodeObjectCreateExpression(typeof(Io.Cucumber.Messages.Types.Rule),
                _location,
                new CodeObjectCreateExpression(tagsListExpr, tagsinitializer),
                new CodePrimitiveExpression(rule.Keyword),
                new CodePrimitiveExpression(rule.Name),
                new CodePrimitiveExpression(rule.Description),
                new CodeObjectCreateExpression(ruleChildrenListExpr, ruleChildrenInitializer),
                new CodePrimitiveExpression(rule.Id));

            _location = location;
            _RuleChildrenList = ruleChildren;
            _TagsList = tags;
        }

        public override void Visit(RuleChild ruleChild)
        {
            var background = _background;
            var scenario = _scenario;

            _background = null;
            _scenario = null;

            base.Visit(ruleChild);

            _RuleChildrenList.Add(new CodeObjectCreateExpression(typeof(RuleChild),
                _background ?? new CodePrimitiveExpression(null),
                _scenario ?? new CodePrimitiveExpression(null)));

            _background = background;
            _scenario = scenario;
        }

        public override void Visit(Scenario scenario)
        {
            var location = _location;
            var tags = _TagsList;
            var steps = _StepsList;
            var examples = _ExamplesList;
            _TagsList = new List<CodeExpression>();
            _StepsList = new List<CodeExpression>();
            _ExamplesList = new List<CodeExpression>();

            base.Visit(scenario);

            var tagsListExpr = new CodeTypeReference(typeof(List<Tag>));
            var tagsinitializer = new CodeArrayCreateExpression(typeof(Tag), _TagsList.ToArray());

            var stepsListExpr = new CodeTypeReference(typeof(List<Step>));
            var stepsinitializer = new CodeArrayCreateExpression(typeof(Step), _StepsList.ToArray());

            var examplesListExpr = new CodeTypeReference(typeof(List<Examples>));
            var examplesinitializer = new CodeArrayCreateExpression(typeof(Examples), _ExamplesList.ToArray());

            _scenario = new CodeObjectCreateExpression(typeof(Scenario),
                _location,
                new CodeObjectCreateExpression(tagsListExpr, tagsinitializer),
                new CodePrimitiveExpression(scenario.Keyword),
                new CodePrimitiveExpression(scenario.Name),
                new CodePrimitiveExpression(scenario.Description),
                new CodeObjectCreateExpression(stepsListExpr, stepsinitializer),
                new CodeObjectCreateExpression(examplesListExpr, examplesinitializer),
                new CodePrimitiveExpression(scenario.Id));

            _location = location;
            _TagsList = tags;
            _StepsList = steps;
            _ExamplesList = examples;
        }

        public override void Visit(Examples examples)
        {
            var location = _location;
            var tags = _TagsList;
            var table = _TableRowsList;
            _TagsList = new List<CodeExpression>();
            _TableRowsList = new List<CodeExpression>();

            // When visting Examples, all TableRow intances that get visited (both TableHeaderRow and TableBodyRows) will get added to the _TableRowsList.
            // Therefore, when we create the Examples create expression, we'll pull the Header out of the _TableRowsList as the first item
            // and the Body out of the _TableRowsList as the rest of the items.

            base.Visit(examples);

            var tagsListExpr = new CodeTypeReference(typeof(List<Tag>));
            var tagsinitializer = new CodeArrayCreateExpression(typeof(Tag), _TagsList.ToArray());
            var tableHeaderRow = _TableRowsList.First();
            var tableBodyListExpr = new CodeTypeReference(typeof(List<TableRow>));
            var tableBodyInitializer = new CodeArrayCreateExpression(typeof(TableRow), _TableRowsList.Skip(1).ToArray());

            _ExamplesList.Add(new CodeObjectCreateExpression(typeof(Examples),
                _location,
                new CodeObjectCreateExpression(tagsListExpr, tagsinitializer),
                new CodePrimitiveExpression(examples.Keyword),
                new CodePrimitiveExpression(examples.Name),
                new CodePrimitiveExpression(examples.Description),
                tableHeaderRow,
                new CodeObjectCreateExpression(tableBodyListExpr, tableBodyInitializer),
                new CodePrimitiveExpression(examples.Id)));

            _location = location;
            _TagsList = tags;
            _TableRowsList = table;
        }

        public override void Visit(Background background)
        {
            var location = _location;
            var steps = _StepsList;
            _StepsList = new List<CodeExpression>();

            base.Visit(background);
            var stepListExpr = new CodeTypeReference(typeof(List<Step>));
            var initializer = new CodeArrayCreateExpression(typeof(Step), _StepsList.ToArray());

            _background = new CodeObjectCreateExpression(typeof(Background),
                _location,
                new CodePrimitiveExpression(background.Keyword),
                new CodePrimitiveExpression(background.Name),
                new CodePrimitiveExpression(background.Description),
                new CodeObjectCreateExpression(stepListExpr, initializer),
                new CodePrimitiveExpression(background.Id));

            _location = location;
            _StepsList = steps;
        }

        public override void Visit(Step step)
        {
            var location = _location;
            var docString = _DocString;
            var dataTable = _dataTable;

            _DocString = null;
            _dataTable = null;

            base.Visit(step);

            _StepsList.Add(new CodeObjectCreateExpression(typeof(Step),
                _location,
                new CodePrimitiveExpression(step.Keyword),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(StepKeywordType)), step.KeywordType.ToString()),
                new CodePrimitiveExpression(step.Text),
                _DocString ?? new CodePrimitiveExpression(null),
                _dataTable ?? new CodePrimitiveExpression(null),
                new CodePrimitiveExpression(step.Id)));

            _location = location;
            _DocString = docString;
            _dataTable = dataTable;
        }

        public override void Visit(DocString docString)
        {
            var location = _location;

            base.Visit(docString);

            _DocString = new CodeObjectCreateExpression(typeof(DocString),
                _location,
                new CodePrimitiveExpression(docString.MediaType),
                new CodePrimitiveExpression(docString.Content),
                new CodePrimitiveExpression(docString.Delimiter));

            _location = location;
        }

        public override void Visit(Io.Cucumber.Messages.Types.DataTable dataTable)
        {
            var location = _location;
            var rows = _TableRowsList;
            _TableRowsList = new List<CodeExpression>();

            base.Visit(dataTable);

            var listExpr = new CodeTypeReference(typeof(List<TableRow>));
            var initializer = new CodeArrayCreateExpression(typeof(TableRow), _TableRowsList.ToArray());

            _dataTable = new CodeObjectCreateExpression(typeof(Io.Cucumber.Messages.Types.DataTable),
                _location,
                new CodeObjectCreateExpression(listExpr, initializer));

            _location = location;
            _TableRowsList = rows;
        }

        public override void Visit(TableRow row)
        {
            var location = _location;
            var cells = _TableCellsList;
            _TableCellsList = new List<CodeExpression>();

            base.Visit(row);

            var CellListExpr = new CodeTypeReference(typeof(List<TableCell>));

            var initializer = new CodeArrayCreateExpression(typeof(TableCell), _TableCellsList.ToArray());

            _TableRowsList.Add(new CodeObjectCreateExpression(typeof(TableRow),
                _location,
                new CodeObjectCreateExpression(CellListExpr, initializer),
                new CodePrimitiveExpression(row.Id)));

            _location = location;
            _TableCellsList = cells;
        }

        public override void Visit(TableCell cell)
        {
            var location = _location;

            base.Visit(cell);

            _TableCellsList.Add(new CodeObjectCreateExpression(typeof(TableCell),
                _location,
                new CodePrimitiveExpression(cell.Value)));

            _location = location;
        }
    }
}
