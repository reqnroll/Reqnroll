using Gherkin.CucumberMessages.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Reqnroll.CucumberMessages.PayloadProcessing.Gherkin
{
    abstract class GherkinTypesPickleVisitor
    {

        public virtual void AcceptPickle(Pickle pickle)
        {
            OnVisitingPickle(pickle);

            foreach (var tag in pickle.Tags)
            {
                AcceptTag(tag);
            }
            foreach (var step in pickle.Steps)
            {
                AcceptStep(step);
            }
            OnVisitedPickle(pickle);
        }

        protected virtual void AcceptStep(PickleStep step)
        {
            OnVisitingPickleStep(step);
            AcceptPickleStepArgument(step.Argument);
            OnVisitedPickleStep(step);
        }

        protected virtual void AcceptPickleStepArgument(PickleStepArgument argument)
        {
            OnVisitingPickleStepArgument(argument);
            AcceptPickleTable(argument.DataTable);
            OnVisitedPickleStepArgument(argument);
        }

        protected virtual void AcceptPickleTable(PickleTable dataTable)
        {
            OnVisitingPickleTable(dataTable);
            foreach (var row in dataTable.Rows)
            {
                AcceptPickleTableRow(row);
            }
            OnVisitedPickleTable(dataTable);
        }

        protected virtual void AcceptPickleTableRow(PickleTableRow row)
        {
            OnVisitingPickleTableRow(row);
            foreach (var cell in row.Cells)
            {
                AcceptPickleTableCell(cell);
            }
            OnVisitedPickleTableRow(row);
        }

        protected virtual void AcceptPickleTableCell(PickleTableCell cell)
        {
            OnVisitedPickleTableCell(cell);
        }
        protected virtual void AcceptTag(PickleTag tag)
        {
            OnVisitedPickleTag(tag);
        }

        protected virtual void OnVisitingPickle(Pickle pickle)
        {
            //nop
        }

        protected virtual void OnVisitedPickle(Pickle pickle)
        {
            //nop
        }

        protected virtual void OnVisitedPickleTag(PickleTag tag)
        {
            //nop
        }

        protected virtual void OnVisitingPickleStep(PickleStep step)
        {
            //nop
        }

        protected virtual void OnVisitedPickleStep(PickleStep step)
        {
            //nop
        }

        protected virtual void OnVisitingPickleStepArgument(PickleStepArgument argument)
        {
            //nop
        }

        protected virtual void OnVisitedPickleStepArgument(PickleStepArgument argument)
        {
            //nop   
        }

        protected virtual void OnVisitingPickleTable(PickleTable dataTable)
        {
            //nop
        }

        protected virtual void OnVisitedPickleTable(PickleTable dataTable)
        {
            //nop
        }

        protected virtual void OnVisitingPickleTableRow(PickleTableRow row)
        {
            //nop
        }

        protected virtual void OnVisitedPickleTableRow(PickleTableRow row)
        {
            //nop
        }

        protected virtual void OnVisitedPickleTableCell(PickleTableCell cell)
        {
            //nop
        }
    }
}
