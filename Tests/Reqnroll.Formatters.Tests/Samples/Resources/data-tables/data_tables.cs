using Reqnroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace CucumberMessages.CompatibilityTests.CCK.data_tables
{
    [Binding]
    internal class data_tables
    {
        private Table? _transposedTable;

        [When("the following table is transposed:")]
        public void WhenTheFollowingTableIsTransposed(Table table)
        {
            _transposedTable = Transpose(table);
        }

        [Then("it should be:")]
        public void ThenItShouldBe(Table expected)
        {
            TablesEqual(expected, _transposedTable!);
        }

        private void TablesEqual(Table expected, Table transposedTable)
        {
            expected = MakeHeaderLess(expected);
            var ExpectednumRows = expected.Rows.Count;
            var ExpectednumCols = expected.Rows[0].Count;

            if (ExpectednumRows != transposedTable.Rows.Count || ExpectednumCols != transposedTable.Rows[0].Count)
            {
                throw new Exception("Tables are not equal");
            }

            for (int i = 0; i < ExpectednumRows; i++)
            {
                for (int j = 0; j < ExpectednumCols; j++)
                {
                    if (expected.Rows[i][j].ToString() != transposedTable.Rows[i][j].ToString())
                    {
                        throw new Exception("Tables are not equal");
                    }
                }
            }
        }

        private Table Transpose(Table table)
        {
            Table tempTable = MakeHeaderLess(table);

            string[][] matrix = GetStringArray(tempTable.Rows);
            var t = TransposeMatrix(matrix);
            return CreateTable(t);

            static string[][] GetStringArray(DataTableRows rows)
            {
                int numRows = rows.Count;
                int numCols = rows.FirstOrDefault()?.Count ?? 0;

                string[][] result = new string[numRows][];
                for (int i = 0; i < numRows; i++)
                {
                    result[i] = new string[numCols];
                    for (int j = 0; j < numCols; j++)
                    {
                        result[i][j] = rows[i][j].ToString();
                    }
                }

                return result;
            }
            static string[][] TransposeMatrix(string[][] matrix)
            {
                int numRows = matrix.Length;
                int numCols = matrix[0].Length;

                string[][] transposedMatrix = new string[numCols][];
                for (int i = 0; i < numCols; i++)
                {
                    transposedMatrix[i] = new string[numRows];
                }

                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        transposedMatrix[j][i] = matrix[i][j];
                    }
                }

                return transposedMatrix;
            }
            static Table CreateTable(string[][] matrix)
            {
                var columnCount = matrix[0].Length;
                var headers = Enumerable.Range(0, columnCount).Select(i => $"").ToArray();
                var table = new Table(headers);

                foreach (var row in matrix)
                {
                    table.AddRow(row);
                }

                return table;
            }
        }

        private static Table MakeHeaderLess(Table table)
        {
            // push the header into a new Table as the first row of that table
            var header = table.Header;
            var tempTable = new Table(Enumerable.Range(0, header.Count).Select(i => $"").ToArray());
            tempTable.AddRow(header.ToArray());
            foreach (var row in table.Rows)
            {
                tempTable.AddRow(row.Select(kvp => kvp.Value).ToArray());
            }

            return tempTable;
        }
    }
}
