using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Io.Cucumber.Messages.Types;
using System;
using System.Linq;
using System.CodeDom;
using System.Collections.Generic;

namespace Reqnroll.Generator.Generation;

/// <summary>
/// This class is responsible for generating a CodeDom expression that can recreate the given Cucumber Gherkin Document.
/// </summary>
internal class CucumberGherkinDocumentExpressionGenerator : CucumberMessage_TraversalVisitorBase
{
    private GherkinDocument _gherkinDocument;
    private CodeExpression _gherkinDocumentExpression;
    private CodeExpression _feature;
    private List<CodeExpression> _commentsList;
    private CodeExpression _location;
    private List<CodeExpression> _tagsList;
    private List<CodeExpression> _featureChildrenList;
    private CodeExpression _background;
    private CodeExpression _scenario;
    private CodeExpression _rule;
    private List<CodeExpression> _ruleChildrenList;
    private List<CodeExpression> _stepsList;
    private List<CodeExpression> _examplesList;
    private CodeExpression _dataTable;
    private CodeExpression _docString;
    private List<CodeExpression> _tableRowsList;
    private List<CodeExpression> _tableCellsList;

    private static readonly string GenericList = typeof(List<>).FullName;

    private void Reset()
    {
        _feature = null;
        _commentsList = new();
        _location = null;
        _tagsList = new();
        _featureChildrenList = new();
        _background = null;
        _scenario = null;
        _rule = null;
        _ruleChildrenList = new();
        _stepsList = new();
        _examplesList = new();
        _dataTable = null;
        _docString = null;
        _tableRowsList = new();
        _tableCellsList = new();

    }

    public CodeExpression GenerateGherkinDocumentExpression(GherkinDocument gherkinDocument)
    {
        Reset();

        _gherkinDocument = gherkinDocument;
        _commentsList = new List<CodeExpression>();

        Visit(gherkinDocument);

        var commentTypeRef = new CodeTypeReference(typeof(Comment), CodeTypeReferenceOptions.GlobalReference);
        var commentsListExpr = new CodeTypeReference(GenericList, commentTypeRef);
        var initializer = new CodeArrayCreateExpression(commentTypeRef, _commentsList.ToArray());

        _gherkinDocumentExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(GherkinDocument), CodeTypeReferenceOptions.GlobalReference),
                                                                    new CodePrimitiveExpression(_gherkinDocument.Uri),
                                                                    _feature,
                                                                    new CodeObjectCreateExpression(commentsListExpr, initializer));

        return _gherkinDocumentExpression;
    }


    public override void Visit(Feature feature)
    {
        var location = _location;
        var featureChildren = _featureChildrenList;
        _featureChildrenList = new List<CodeExpression>();
        var tags = _tagsList;
        _tagsList = new List<CodeExpression>();

        base.Visit(feature);

        var tagCodeDomTypeRef = new CodeTypeReference(typeof(Tag), CodeTypeReferenceOptions.GlobalReference);
        var tagsListExpr = new CodeTypeReference(GenericList, tagCodeDomTypeRef);
        var tagsInitializer = new CodeArrayCreateExpression(tagCodeDomTypeRef, _tagsList.ToArray());

        var fcCodeDomTypeRef = new CodeTypeReference(typeof(FeatureChild), CodeTypeReferenceOptions.GlobalReference);
        var fcListExpr = new CodeTypeReference(GenericList, fcCodeDomTypeRef);
        var initializer = new CodeArrayCreateExpression(fcCodeDomTypeRef, _featureChildrenList.ToArray());

        _feature = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Feature), CodeTypeReferenceOptions.GlobalReference),
                                                  _location,
                                                  new CodeObjectCreateExpression(tagsListExpr, tagsInitializer),
                                                  new CodePrimitiveExpression(feature.Language),
                                                  new CodePrimitiveExpression(feature.Keyword),
                                                  new CodePrimitiveExpression(feature.Name),
                                                  new CodePrimitiveExpression(feature.Description),
                                                  new CodeObjectCreateExpression(fcListExpr, initializer));

        _location = location;
        _featureChildrenList = featureChildren;
        _tagsList = tags;
    }

    public override void Visit(Comment comment)
    {
        var location = _location;

        base.Visit(comment);

        _commentsList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(Comment), CodeTypeReferenceOptions.GlobalReference),
                                                         _location,
                                                         new CodePrimitiveExpression(comment.Text)));

        _location = location;
    }

    public override void Visit(Tag tag)
    {
        var location = _location;

        base.Visit(tag);

        _tagsList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(Tag), CodeTypeReferenceOptions.GlobalReference),
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

        _location = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Location), CodeTypeReferenceOptions.GlobalReference),
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

        _featureChildrenList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(FeatureChild), CodeTypeReferenceOptions.GlobalReference),
                                                                _rule ?? new CodePrimitiveExpression(null),
                                                                _background ?? new CodePrimitiveExpression(null),
                                                                _scenario ?? new CodePrimitiveExpression(null)));

        _rule = rule;
        _scenario = scenario;
        _background = background;
    }

    public override void Visit(Rule rule)
    {
        var location = _location;
        var ruleChildren = _ruleChildrenList;
        _ruleChildrenList = new();
        var tags = _tagsList;
        _tagsList = new();

        base.Visit(rule);

        var tagCodeDomTypeRef = new CodeTypeReference(typeof(Tag), CodeTypeReferenceOptions.GlobalReference);
        var tagsListExpr = new CodeTypeReference(GenericList, tagCodeDomTypeRef);
        var tagsInitializer = new CodeArrayCreateExpression(tagCodeDomTypeRef, _tagsList.ToArray());

        var ruleChildCodeDomTypeRef = new CodeTypeReference(typeof(RuleChild), CodeTypeReferenceOptions.GlobalReference);
        var ruleChildrenListExpr = new CodeTypeReference(GenericList, ruleChildCodeDomTypeRef);
        var ruleChildrenInitializer = new CodeArrayCreateExpression(ruleChildCodeDomTypeRef, _ruleChildrenList.ToArray());

        _rule = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Rule), CodeTypeReferenceOptions.GlobalReference),
                                               _location,
                                               new CodeObjectCreateExpression(tagsListExpr, tagsInitializer),
                                               new CodePrimitiveExpression(rule.Keyword),
                                               new CodePrimitiveExpression(rule.Name),
                                               new CodePrimitiveExpression(rule.Description),
                                               new CodeObjectCreateExpression(ruleChildrenListExpr, ruleChildrenInitializer),
                                               new CodePrimitiveExpression(rule.Id));

        _location = location;
        _ruleChildrenList = ruleChildren;
        _tagsList = tags;
    }

    public override void Visit(RuleChild ruleChild)
    {
        var background = _background;
        var scenario = _scenario;

        _background = null;
        _scenario = null;

        base.Visit(ruleChild);

        _ruleChildrenList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(RuleChild), CodeTypeReferenceOptions.GlobalReference),
                                                             _background ?? new CodePrimitiveExpression(null),
                                                             _scenario ?? new CodePrimitiveExpression(null)));

        _background = background;
        _scenario = scenario;
    }

    public override void Visit(Scenario scenario)
    {
        var location = _location;
        var tags = _tagsList;
        var steps = _stepsList;
        var examples = _examplesList;
        _tagsList = new();
        _stepsList = new();
        _examplesList = new();

        base.Visit(scenario);

        var tagCodeDomTypeRef = new CodeTypeReference(typeof(Tag), CodeTypeReferenceOptions.GlobalReference);
        var tagsListExpr = new CodeTypeReference(GenericList, tagCodeDomTypeRef);
        var tagsInitializer = new CodeArrayCreateExpression(tagCodeDomTypeRef, _tagsList.ToArray());

        var stepCodeDomTypeRef = new CodeTypeReference(typeof(Step), CodeTypeReferenceOptions.GlobalReference);
        var stepsListExpr = new CodeTypeReference(GenericList, stepCodeDomTypeRef);
        var stepsInitializer = new CodeArrayCreateExpression(stepCodeDomTypeRef, _stepsList.ToArray());

        var examplesCodeDomTypeRef = new CodeTypeReference(typeof(Examples), CodeTypeReferenceOptions.GlobalReference);
        var examplesListExpr = new CodeTypeReference(GenericList, examplesCodeDomTypeRef);
        var examplesInitializer = new CodeArrayCreateExpression(examplesCodeDomTypeRef, _examplesList.ToArray());

        _scenario = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Scenario), CodeTypeReferenceOptions.GlobalReference),
                                                   _location,
                                                   new CodeObjectCreateExpression(tagsListExpr, tagsInitializer),
                                                   new CodePrimitiveExpression(scenario.Keyword),
                                                   new CodePrimitiveExpression(scenario.Name),
                                                   new CodePrimitiveExpression(scenario.Description),
                                                   new CodeObjectCreateExpression(stepsListExpr, stepsInitializer),
                                                   new CodeObjectCreateExpression(examplesListExpr, examplesInitializer),
                                                   new CodePrimitiveExpression(scenario.Id));

        _location = location;
        _tagsList = tags;
        _stepsList = steps;
        _examplesList = examples;
    }

    public override void Visit(Examples examples)
    {
        var location = _location;
        var tags = _tagsList;
        var table = _tableRowsList;
        _tagsList = new();
        _tableRowsList = new();

        // When visiting Examples, all TableRow instances that get visited (both TableHeaderRow and TableBodyRows) will get added to the _tableRowsList.
        // Therefore, when we create the Examples create expression, we'll pull the Header out of the _tableRowsList as the first item
        // and the Body out of the _tableRowsList as the rest of the items.

        base.Visit(examples);

        var tagCodeDomTypeRef = new CodeTypeReference(typeof(Tag), CodeTypeReferenceOptions.GlobalReference);
        var tagsListExpr = new CodeTypeReference(GenericList, tagCodeDomTypeRef);
        var tagsInitializer = new CodeArrayCreateExpression(tagCodeDomTypeRef, _tagsList.ToArray());
        var tableHeaderRow = _tableRowsList.First();

        var tableRowCodeDomTypeRef = new CodeTypeReference(typeof(TableRow), CodeTypeReferenceOptions.GlobalReference);
        var tableBodyListExpr = new CodeTypeReference(GenericList, tableRowCodeDomTypeRef);
        var tableBodyInitializer = new CodeArrayCreateExpression(tableRowCodeDomTypeRef, _tableRowsList.Skip(1).ToArray());

        _examplesList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(Examples), CodeTypeReferenceOptions.GlobalReference),
                                                         _location,
                                                         new CodeObjectCreateExpression(tagsListExpr, tagsInitializer),
                                                         new CodePrimitiveExpression(examples.Keyword),
                                                         new CodePrimitiveExpression(examples.Name),
                                                         new CodePrimitiveExpression(examples.Description),
                                                         tableHeaderRow,
                                                         new CodeObjectCreateExpression(tableBodyListExpr, tableBodyInitializer),
                                                         new CodePrimitiveExpression(examples.Id)));

        _location = location;
        _tagsList = tags;
        _tableRowsList = table;
    }

    public override void Visit(Background background)
    {
        var location = _location;
        var steps = _stepsList;
        _stepsList = new();

        base.Visit(background);

        var stepCodeDomTypeRef = new CodeTypeReference(typeof(Step), CodeTypeReferenceOptions.GlobalReference);
        var stepListExpr = new CodeTypeReference(GenericList, stepCodeDomTypeRef);
        var initializer = new CodeArrayCreateExpression(stepCodeDomTypeRef, _stepsList.ToArray());

        _background = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Background), CodeTypeReferenceOptions.GlobalReference),
                                                     _location,
                                                     new CodePrimitiveExpression(background.Keyword),
                                                     new CodePrimitiveExpression(background.Name),
                                                     new CodePrimitiveExpression(background.Description),
                                                     new CodeObjectCreateExpression(stepListExpr, initializer),
                                                     new CodePrimitiveExpression(background.Id));

        _location = location;
        _stepsList = steps;
    }

    public override void Visit(Step step)
    {
        var location = _location;
        var docString = _docString;
        var dataTable = _dataTable;

        _docString = null;
        _dataTable = null;

        base.Visit(step);

        _stepsList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(Step), CodeTypeReferenceOptions.GlobalReference),
                                                      _location,
                                                      new CodePrimitiveExpression(step.Keyword),
                                                      new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(typeof(StepKeywordType), CodeTypeReferenceOptions.GlobalReference)), step.KeywordType.ToString()),
                                                      new CodePrimitiveExpression(step.Text),
                                                      _docString ?? new CodePrimitiveExpression(null),
                                                      _dataTable ?? new CodePrimitiveExpression(null),
                                                      new CodePrimitiveExpression(step.Id)));

        _location = location;
        _docString = docString;
        _dataTable = dataTable;
    }

    public override void Visit(DocString docString)
    {
        var location = _location;

        base.Visit(docString);

        _docString = new CodeObjectCreateExpression(new CodeTypeReference(typeof(DocString), CodeTypeReferenceOptions.GlobalReference),
                                                    _location,
                                                    new CodePrimitiveExpression(docString.MediaType),
                                                    new CodePrimitiveExpression(docString.Content),
                                                    new CodePrimitiveExpression(docString.Delimiter));

        _location = location;
    }

    public override void Visit(Io.Cucumber.Messages.Types.DataTable dataTable)
    {
        var location = _location;
        var rows = _tableRowsList;
        _tableRowsList = new();

        base.Visit(dataTable);

        var tableRowCodeDomTypeRef = new CodeTypeReference(typeof(TableRow), CodeTypeReferenceOptions.GlobalReference);
        var listExpr = new CodeTypeReference(GenericList, tableRowCodeDomTypeRef);
        var initializer = new CodeArrayCreateExpression(tableRowCodeDomTypeRef, _tableRowsList.ToArray());

        _dataTable = new CodeObjectCreateExpression(new CodeTypeReference(typeof(Io.Cucumber.Messages.Types.DataTable), CodeTypeReferenceOptions.GlobalReference),
                                                    _location,
                                                    new CodeObjectCreateExpression(listExpr, initializer));

        _location = location;
        _tableRowsList = rows;
    }

    public override void Visit(TableRow row)
    {
        var location = _location;
        var cells = _tableCellsList;
        _tableCellsList = new();

        base.Visit(row);

        var tableCellCodeDomTypeRef = new CodeTypeReference(typeof(TableCell), CodeTypeReferenceOptions.GlobalReference);
        var cellListExpr = new CodeTypeReference(GenericList, tableCellCodeDomTypeRef);

        var initializer = new CodeArrayCreateExpression(tableCellCodeDomTypeRef, _tableCellsList.ToArray());

        _tableRowsList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(TableRow), CodeTypeReferenceOptions.GlobalReference),
                                                          _location,
                                                          new CodeObjectCreateExpression(cellListExpr, initializer),
                                                          new CodePrimitiveExpression(row.Id)));

        _location = location;
        _tableCellsList = cells;
    }

    public override void Visit(TableCell cell)
    {
        var location = _location;

        base.Visit(cell);

        _tableCellsList.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(TableCell), CodeTypeReferenceOptions.GlobalReference),
                                                           _location,
                                                           new CodePrimitiveExpression(cell.Value)));

        _location = location;
    }
}