using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Generator.CodeDom;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reqnroll.Generator.Generation
{
    /// <summary>
    /// Generates a CodeDom expression to create a list of Cucumber Pickles
    /// </summary>
    internal class CucumberPicklesExpressionGenerator : CucumberMessage_TraversalVisitorBase
    {
        private CodeDomHelper _codeDomHelper;
        private List<CodeExpression> _PickleList;
        private List<CodeExpression> _PickleSteps;
        private List<CodeExpression> _PickleTags;
        CodeExpression _PickleStepArgument;
        CodeExpression _PickleDocString;
        CodeExpression _PickleTable;
        private List<CodeExpression> _TableRows;
        private List<CodeExpression> _PickleCells;

        public CucumberPicklesExpressionGenerator(CodeDomHelper codeDomHelper)
        {
            _codeDomHelper = codeDomHelper;
        }

        private void Reset()
        {
            _PickleList = new List<CodeExpression>();
            _PickleSteps = new List<CodeExpression>();
            _PickleTags = new List<CodeExpression>();
            _PickleStepArgument = null;
            _PickleDocString = null;
            _PickleTable = null;
        }

        public CodeExpression GeneratePicklesExpression(IEnumerable<Pickle> pickles)
        {
            Reset();
            foreach (var pickle in pickles)
            {
                Visit(pickle);
            }
            var commentsListExpr = new CodeTypeReference(typeof(List<Pickle>));
            var initializer = new CodeArrayCreateExpression(typeof(Pickle), _PickleList.ToArray());

            return new CodeObjectCreateExpression(commentsListExpr, initializer);
        }

        public override void Visit(Pickle pickle)
        {
            var steps = _PickleSteps;
            _PickleSteps = new List<CodeExpression>();

            var tags = _PickleTags;
            _PickleTags = new List<CodeExpression>();

            base.Visit(pickle);

            var stepsExpr = new CodeTypeReference(typeof(List<PickleStep>));
            var stepsinitializer = new CodeArrayCreateExpression(typeof(PickleStep), _PickleSteps.ToArray());

            var tagsExpr = new CodeTypeReference(typeof(List<PickleTag>));
            var tagsinitializer = new CodeArrayCreateExpression(typeof(PickleTag), _PickleTags.ToArray());

            var astIdsExpr = new CodeTypeReference(typeof(List<string>));
            var astIdsInitializer = new CodeArrayCreateExpression(typeof(string), pickle.AstNodeIds.Select(s => new CodePrimitiveExpression(s)).ToArray());

            _PickleList.Add(new CodeObjectCreateExpression(typeof(Pickle),
                            new CodePrimitiveExpression(pickle.Id),
                            new CodePrimitiveExpression(pickle.Uri),
                            new CodePrimitiveExpression(pickle.Name),
                            new CodePrimitiveExpression(pickle.Language),
                            new CodeObjectCreateExpression(stepsExpr, stepsinitializer),
                            new CodeObjectCreateExpression(tagsExpr, tagsinitializer),
                            new CodeObjectCreateExpression(astIdsExpr, astIdsInitializer)
                            ));

            _PickleSteps = steps;
            _PickleTags = tags;
        }

        public override void Visit(PickleStep step)
        {
            var arg = _PickleStepArgument;
            _PickleStepArgument = null;

            base.Visit(step);

            var astIdsExpr = new CodeTypeReference(typeof(List<string>));
            var astIdsInitializer = new CodeArrayCreateExpression(typeof(string), step.AstNodeIds.Select(s => new CodePrimitiveExpression(s)).ToArray());

            _PickleSteps.Add(new CodeObjectCreateExpression(typeof(PickleStep),
                _PickleStepArgument ?? new CodePrimitiveExpression(null),
                new CodeObjectCreateExpression(astIdsExpr, astIdsInitializer),
                new CodePrimitiveExpression(step.Id),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PickleStepType)), step.Type.ToString()),
                new CodePrimitiveExpression(step.Text)));

            _PickleStepArgument = arg;
        }

        public override void Visit(PickleDocString docString)
        {
            _PickleDocString = new CodeObjectCreateExpression(typeof(PickleDocString),
                new CodePrimitiveExpression(docString.MediaType),
                new CodePrimitiveExpression(docString.Content));
        }

        public override void Visit(PickleStepArgument argument)
        {
            var docString = _PickleDocString;
            var table = _PickleTable;

            _PickleDocString = null;
            _PickleTable = null;

            base.Visit(argument);

            _PickleStepArgument = new CodeObjectCreateExpression(typeof(PickleStepArgument),
                _PickleDocString ?? new CodePrimitiveExpression(null),
                _PickleTable ?? new CodePrimitiveExpression(null));

            _PickleDocString = docString;
            _PickleTable = table;
        }

        public override void Visit(PickleTable pickleTable)
        {
            var rows = _TableRows;
            _TableRows = new List<CodeExpression>();

            base.Visit(pickleTable);

            var rowsExpr = new CodeTypeReference(typeof(List<PickleTableRow>));
            var rowsInitializer = new CodeArrayCreateExpression(typeof(PickleTableRow), _TableRows.ToArray());

            _PickleTable = new CodeObjectCreateExpression(typeof(PickleTable),
                new CodeObjectCreateExpression(rowsExpr, rowsInitializer));

            _TableRows = rows;
        }

        public override void Visit(PickleTableRow row)
        {
            var cells = _PickleCells;
            _PickleCells = new List<CodeExpression>();

            base.Visit(row);

            var cellsExpr = new CodeTypeReference(typeof(List<PickleTableCell>));
            var cellsInitializer = new CodeArrayCreateExpression(typeof(PickleTableCell), _PickleCells.ToArray());

            _TableRows.Add(new CodeObjectCreateExpression(typeof(PickleTableRow),
                new CodeObjectCreateExpression(cellsExpr, cellsInitializer)));

            _PickleCells = cells;
        }

        public override void Visit(PickleTableCell cell)
        {
            _PickleCells.Add(new CodeObjectCreateExpression(typeof(PickleTableCell),
                new CodePrimitiveExpression(cell.Value)));
        }

        public override void Visit(PickleTag tag)
        {
            _PickleTags.Add(new CodeObjectCreateExpression(typeof(PickleTag), 
                new CodePrimitiveExpression(tag.Name),
                new CodePrimitiveExpression(tag.AstNodeId)));
        }
    }
}
