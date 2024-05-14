using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Reqnroll
{
    /// <summary>
    /// An alias for the <see cref="DataTable"/> class for backwards compatibility. 
    /// </summary>
    [Serializable]
    public class Table
    {
        internal const string ERROR_NO_CELLS_TO_ADD = "No cells to add";
        internal const string ERROR_NO_HEADER_TO_ADD = "No headers to add";
        internal const string ERROR_COLUMN_NAME_NOT_FOUND = "Could not find a column named '{0}' in the table.";
        internal const string ERROR_CELLS_NOT_MATCHING_HEADERS = "The number of cells ({0}) you are trying to add doesn't match the number of columns ({1})";

        private readonly string[] _header;
        private readonly DataTableRows _rows = new();

        public ICollection<string> Header => _header;

        public DataTableRows Rows => _rows;

        public int RowCount => _rows.Count;

        public Table(params string[] header) : this(header, null)
        {
        }

        protected internal Table(string[] header, string[][] dataRows)
        {
            if (header == null || header.Length == 0)
            {
                throw new ArgumentException(ERROR_NO_HEADER_TO_ADD, nameof(header));
            }
            for (int colIndex = 0; colIndex < header.Length; colIndex++)
                header[colIndex] ??= string.Empty;
            _header = header;
            if (dataRows != null)
            {
                foreach (var dataRow in dataRows)
                {
                    _rows.Add(new DataTableRow(this, dataRow));
                }
            }
        }

        protected internal Table(Table copyFrom) : this(copyFrom._header, copyFrom._rows.ToArray())
        {
        }

        public bool ContainsColumn(string column)
        {
            return GetHeaderIndex(column, false) >= 0;
        }

        internal int GetHeaderIndex(string column, bool throwIfNotFound = true)
        {
            int index = Array.IndexOf(_header, column);
            if (!throwIfNotFound)
                return index;
            if (index < 0)
            {
                var mess = string.Format(
                            ERROR_COLUMN_NAME_NOT_FOUND + "\nThe table looks like this:\n{1}",
                            column,
                            this);
                throw new IndexOutOfRangeException(mess);
            }
            return index;
        }

        public void AddRow(IDictionary<string, string> values)
        {
            string[] cells = new string[_header.Length];
            foreach (var value in values)
            {
                int headerIndex = GetHeaderIndex(value.Key);
                cells[headerIndex] = value.Value;
            }

            AddRow(cells);
        }

        public void AddRow(params string[] cells)
        {
            if (cells == null)
                throw new Exception(ERROR_NO_CELLS_TO_ADD);

            if (cells.Length != _header.Length)
            {
                var mess =
                    string.Format(
                        ERROR_CELLS_NOT_MATCHING_HEADERS + ".\nThe table looks like this\n{2}",
                        cells.Length,
                        _header.Length,
                        this);
                throw new ArgumentException(mess);
            }
            var row = new DataTableRow(this, cells);
            _rows.Add(row);
        }

        public void RenameColumn(string oldColumn, string newColumn)
        {
            int colIndex = GetHeaderIndex(oldColumn);
            _header[colIndex] = newColumn;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool headersOnly, bool withNewline = true)
        {
            int[] columnWidths = new int[_header.Length];
            for (int colIndex = 0; colIndex < _header.Length; colIndex++)
                columnWidths[colIndex] = _header[colIndex].Length;

            if (!headersOnly)
            {
                foreach (DataTableRow row in _rows)
                {
                    for (int colIndex = 0; colIndex < _header.Length; colIndex++)
                        columnWidths[colIndex] = Math.Max(columnWidths[colIndex], row[colIndex].Length);
                }
            }

            StringBuilder builder = new StringBuilder();
            AddTableRow(builder, _header, columnWidths);

            if (!headersOnly)
            {
                foreach (DataTableRow row in _rows)
                    AddTableRow(builder, row.Select(pair => pair.Value), columnWidths);
            }

            if (!withNewline)
            {
                var newlineLength = Environment.NewLine.Length;
                builder.Remove(builder.Length - newlineLength, newlineLength);
            }

            return builder.ToString();
        }

        private void AddTableRow(StringBuilder builder, IEnumerable<string> cells, int[] widths)
        {
            const string margin = " ";
            const string separator = "|";
            int colIndex = 0;

            builder.Append(separator);
            foreach (string cell in cells)
            {
                builder.Append(margin);

                builder.Append(cell);
                builder.Append(' ', widths[colIndex] - cell.Length);

                builder.Append(margin);
                builder.Append(separator);

                colIndex++;
            }

            builder.AppendLine();
        }
    }

    [Serializable]
    public class DataTableRows : IEnumerable<DataTableRow>
    {
        private readonly List<DataTableRow> _innerList = new();

        public int Count => _innerList.Count;

        public DataTableRow this[int index] => _innerList[index];

        public IEnumerator<DataTableRow> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal void Add(DataTableRow row)
        {
            _innerList.Add(row);
        }

        internal string[][] ToArray()
        {
            return _innerList.Select(tr => tr.Items).ToArray();
        }
    }

    [Serializable]
    public class DataTableRow : IDictionary<string, string>
    {
        private readonly Table _table;
        private readonly string[] _items;

        internal DataTableRow(Table table, string[] items)
        {
            for (int colIndex = 0; colIndex < items.Length; colIndex++)
                items[colIndex] ??= string.Empty;

            _table = table;
            _items = items;
        }

        public string this[string header]
        {
            get
            {
                int itemIndex = _table.GetHeaderIndex(header);
                return _items[itemIndex];
            }
            set
            {
                int keyIndex = _table.GetHeaderIndex(header);
                _items[keyIndex] = value;
            }
        }

        public string this[int index] => _items[index];

        public int Count => _items.Length;

        internal string[] Items => _items;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            Debug.Assert(_items.Length == _table.Header.Count);
            int itemIndex = 0;
            foreach (string header in _table.Header)
            {
                yield return new KeyValuePair<string, string>(header, _items[itemIndex]);
                itemIndex++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private ReqnrollException ThrowTableStructureCannotBeModified()
        {
            return new ReqnrollException("The table rows must contain the same number of items as the header count of the table. The structure cannot be modified.");
        }

        #region Implementation of ICollection<KeyValuePair<string,string>>

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            int keyIndex = _table.GetHeaderIndex(item.Key, false);
            if (keyIndex < 0)
                return false;
            return _items[keyIndex].Equals(item.Value);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IDictionary<string,string>

        public bool ContainsKey(string key)
        {
            return _table.Header.Contains(key);
        }

        void IDictionary<string, string>.Add(string key, string value)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            throw ThrowTableStructureCannotBeModified();
        }

        public bool TryGetValue(string key, out string value)
        {
            int keyIndex = _table.GetHeaderIndex(key, false);
            if (keyIndex < 0)
            {
                value = null;
                return false;
            }

            value = _items[keyIndex];
            return true;
        }

        public ICollection<string> Keys
        {
            get { return _table.Header; }
        }

        public ICollection<string> Values
        {
            get { return _items; }
        }

        #endregion
    }
}
