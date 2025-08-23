using System.Collections;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static InternalSyntaxFactory;

public partial struct TableCellSyntaxList
{
    public readonly struct CellList(SyntaxNodeOrTokenList<TableCellSyntax> list) : IReadOnlyList<TableCellSyntax>
    {
        public struct Enumerator(CellList list) : IEnumerator<TableCellSyntax>
        {
            private int _index = -1;

            public readonly TableCellSyntax Current => list[_index];

            readonly object? IEnumerator.Current => Current;

            public readonly void Dispose() { }

            public bool MoveNext() => ++_index < list.Count;

            public void Reset() => _index = -1;
        }

        public int Count => (list.Count + 1) / 2;

        public TableCellSyntax this[int index]
        {
            get
            {
                if (list.InternalNode == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                // We want to map indexes to only return the syntax nodes, not the separators.
                // 0, 1, 2, 3 maps to
                // 0, 2, 4, 6 in the internal node list.
                var targetIndex = index * 2;

                return list[targetIndex].AsNode()!;
            }
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<TableCellSyntax> IEnumerable<TableCellSyntax>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TableCellSyntaxList Add(TableCellSyntax node) => Insert(Count, node);

        public TableCellSyntaxList Insert(int index, TableCellSyntax node) => InsertRange(index, new[] { node });

        public TableCellSyntaxList InsertRange(int index, IEnumerable<TableCellSyntax> nodes)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Count == 0)
            {
                // If the list is empty, we can directly create a new list with the nodes.
                return new TableCellSyntaxList(nodes);
            }

            var newList = new InternalSyntaxList<InternalNode?>.Builder();

            // Determine the position in the internal list where the new nodes should be inserted.
            var insertionPoint = index * 2;

            // If the insertion point is not at the start, we need to copy the existing nodes up to that point.
            if (insertionPoint != 0)
            {
                // If the insertion point falls after the end of the list, we copy the entire list and add an additional delimeter.
                if (insertionPoint > list.Count)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        newList.Add(list[i].InternalNode);
                    }

                    newList.Add(Token(SyntaxKind.VerticalBarToken));
                }
                else
                {
                    for (var i = 0; i < insertionPoint; i++)
                    {
                        newList.Add(list[i].InternalNode);
                    }
                }
            }

            // Append the new nodes and separators at the insertion point.
            var first = true;
            foreach (var node in nodes)
            {
                if (!first)
                {
                    newList.Add(Token(SyntaxKind.VerticalBarToken));
                    first = false;
                }

                newList.Add(node.InternalNode);
            }

            // If there are nodes after the insertion point, copy them into the new list.
            if (insertionPoint < list.Count)
            {
                // Separate with a new separator token.
                newList.Add(Token(SyntaxKind.VerticalBarToken));

                for (var i = insertionPoint; i < list.Count; i++)
                {
                    newList.Add(list[i].InternalNode);
                }
            }

            return new TableCellSyntaxList(
                new SyntaxNodeOrTokenList<TableCellSyntax>(newList.ToSyntaxList()));
        }
    }
}
