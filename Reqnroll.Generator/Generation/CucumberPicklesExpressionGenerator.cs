using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Generator.Generation;

/// <summary>
/// Generates a CodeDom expression to create a list of Cucumber Pickles
/// </summary>
internal class CucumberPicklesExpressionGenerator : CucumberMessage_TraversalVisitorBase
{
    private List<CodeExpression> _pickleList;
    private List<CodeExpression> _pickleSteps;
    private List<CodeExpression> _pickleTags;
    private CodeExpression _pickleStepArgument;
    private CodeExpression _pickleDocString;
    private CodeExpression _pickleTable;
    private List<CodeExpression> _tableRows;
    private List<CodeExpression> _pickleCells;

    private static readonly string GenericList = typeof(List<>).FullName;

    private void Reset()
    {
        _pickleList = new();
        _pickleSteps = new();
        _pickleTags = new();
        _pickleStepArgument = null;
        _pickleDocString = null;
        _pickleTable = null;
    }

    public CodeExpression GeneratePicklesExpression(IEnumerable<Pickle> pickles)
    {
        Reset();
        foreach (var pickle in pickles)
        {
            Visit(pickle);
        }

        var pickleCodeTypeRef = new CodeTypeReference(typeof(Pickle), CodeTypeReferenceOptions.GlobalReference);
        var commentsListExpr = new CodeTypeReference(GenericList, pickleCodeTypeRef);
        var initializer = new CodeArrayCreateExpression(pickleCodeTypeRef, _pickleList.ToArray());

        return new CodeObjectCreateExpression(commentsListExpr, initializer);
    }

    public override void Visit(Pickle pickle)
    {
        var steps = _pickleSteps;
        _pickleSteps = new();

        var tags = _pickleTags;
        _pickleTags = new();

        base.Visit(pickle);

        var pStepTypeRef = new CodeTypeReference(typeof(PickleStep), CodeTypeReferenceOptions.GlobalReference);
        var stepsExpr = new CodeTypeReference(GenericList, pStepTypeRef);
        var stepsInitializer = new CodeArrayCreateExpression(pStepTypeRef, _pickleSteps.ToArray());

        var tagsTypeRef = new CodeTypeReference(typeof(PickleTag), CodeTypeReferenceOptions.GlobalReference);
        var tagsExpr = new CodeTypeReference(GenericList, tagsTypeRef);
        var tagsInitializer = new CodeArrayCreateExpression(tagsTypeRef, _pickleTags.ToArray());

        var astIdsExpr = new CodeTypeReference(typeof(List<string>));
        var astIdsInitializer = new CodeArrayCreateExpression(typeof(string), pickle.AstNodeIds.Select(s => new CodePrimitiveExpression(s)).ToArray());

        _pickleList.Add(new CodeObjectCreateExpression(
                            new CodeTypeReference(typeof(Pickle), CodeTypeReferenceOptions.GlobalReference),
                            new CodePrimitiveExpression(pickle.Id),
                            new CodePrimitiveExpression(pickle.Uri),
                            new CodePrimitiveExpression(pickle.Name),
                            new CodePrimitiveExpression(pickle.Language),
                            new CodeObjectCreateExpression(stepsExpr, stepsInitializer),
                            new CodeObjectCreateExpression(tagsExpr, tagsInitializer),
                            new CodeObjectCreateExpression(astIdsExpr, astIdsInitializer)
                        ));

        _pickleSteps = steps;
        _pickleTags = tags;
    }

    public override void Visit(PickleStep step)
    {
        var arg = _pickleStepArgument;
        _pickleStepArgument = null;

        base.Visit(step);

        var astIdsExpr = new CodeTypeReference(typeof(List<string>));
        var astIdsInitializer = new CodeArrayCreateExpression(typeof(string), step.AstNodeIds.Select(s => (CodeExpression)new CodePrimitiveExpression(s)).ToArray());

        _pickleSteps.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleStep), CodeTypeReferenceOptions.GlobalReference),
                                                        _pickleStepArgument ?? new CodePrimitiveExpression(null),
                                                        new CodeObjectCreateExpression(astIdsExpr, astIdsInitializer),
                                                        new CodePrimitiveExpression(step.Id),
                                                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(new CodeTypeReference(typeof(PickleStepType), CodeTypeReferenceOptions.GlobalReference)), step.Type.ToString()),
                                                        new CodePrimitiveExpression(step.Text)));

        _pickleStepArgument = arg;
    }

    public override void Visit(PickleDocString docString)
    {
        _pickleDocString = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleDocString), CodeTypeReferenceOptions.GlobalReference),
                                                          new CodePrimitiveExpression(docString.MediaType),
                                                          new CodePrimitiveExpression(docString.Content));
    }

    public override void Visit(PickleStepArgument argument)
    {
        var docString = _pickleDocString;
        var table = _pickleTable;

        _pickleDocString = null;
        _pickleTable = null;

        base.Visit(argument);

        _pickleStepArgument = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleStepArgument), CodeTypeReferenceOptions.GlobalReference),
                                                             _pickleDocString ?? new CodePrimitiveExpression(null),
                                                             _pickleTable ?? new CodePrimitiveExpression(null));

        _pickleDocString = docString;
        _pickleTable = table;
    }

    public override void Visit(PickleTable pickleTable)
    {
        var rows = _tableRows;
        _tableRows = new();

        base.Visit(pickleTable);

        var pickleTableRowTypeRef = new CodeTypeReference(typeof(PickleTableRow), CodeTypeReferenceOptions.GlobalReference);
        var rowsExpr = new CodeTypeReference(GenericList, pickleTableRowTypeRef);
        var rowsInitializer = new CodeArrayCreateExpression(pickleTableRowTypeRef, _tableRows.ToArray());

        _pickleTable = new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleTable), CodeTypeReferenceOptions.GlobalReference),
                                                      new CodeObjectCreateExpression(rowsExpr, rowsInitializer));

        _tableRows = rows;
    }

    public override void Visit(PickleTableRow row)
    {
        var cells = _pickleCells;
        _pickleCells = new();

        base.Visit(row);

        var pickleTableCellTypeRef = new CodeTypeReference(typeof(PickleTableCell), CodeTypeReferenceOptions.GlobalReference);
        var cellsExpr = new CodeTypeReference(GenericList, pickleTableCellTypeRef);
        var cellsInitializer = new CodeArrayCreateExpression(pickleTableCellTypeRef, _pickleCells.ToArray());

        _tableRows.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleTableRow), CodeTypeReferenceOptions.GlobalReference),
                                                      new CodeObjectCreateExpression(cellsExpr, cellsInitializer)));

        _pickleCells = cells;
    }

    public override void Visit(PickleTableCell cell)
    {
        _pickleCells.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleTableCell), CodeTypeReferenceOptions.GlobalReference),
                                                        new CodePrimitiveExpression(cell.Value)));
    }

    public override void Visit(PickleTag tag)
    {
        _pickleTags.Add(new CodeObjectCreateExpression(new CodeTypeReference(typeof(PickleTag), CodeTypeReferenceOptions.GlobalReference),
                                                       new CodePrimitiveExpression(tag.Name),
                                                       new CodePrimitiveExpression(tag.AstNodeId)));
    }
}