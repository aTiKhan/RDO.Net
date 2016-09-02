﻿using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Windows.Primitives
{
    /// <summary>Normalizes rows if recursive.</summary>
    internal abstract class RowNormalizer : RowMapper
    {
        protected RowNormalizer(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy)
            : base(template, dataSet, where, orderBy)
        {
        }

        private List<RowPresenter> _rows;
        public override IReadOnlyList<RowPresenter> Rows
        {
            get { return _rows; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            _rows = IsRecursive ? new List<RowPresenter>() : (List<RowPresenter>)base.Rows;
            if (IsRecursive)
            {
                foreach (var row in base.Rows)
                    _rows.Add(row);
            }
            UpdateIndex(0);
        }

        private void UpdateIndex(int startIndex)
        {
            for (int i = startIndex; i < _rows.Count; i++)
                _rows[i].RawIndex = i;
        }

        protected virtual void OnRowsChanged()
        {
        }

        internal void Expand(RowPresenter row)
        {
            Debug.Assert(IsRecursive && !row.IsExpanded);

            var nextIndex = row.RawIndex + 1;
            var lastIndex = nextIndex;
            for (int i = 0; i < row.Children.Count; i++)
            {
                var childRow = row.Children[i];
                lastIndex = InsertRecursively(lastIndex, childRow);
            }

            if (lastIndex == nextIndex)
                return;

            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int InsertRecursively(int index, RowPresenter row)
        {
            Debug.Assert(IsRecursive);

            _rows.Insert(index++, row);
            if (row.IsExpanded)
            {
                var children = row.Children;
                foreach (var childRow in children)
                    index = InsertRecursively(index, childRow);
            }
            return index;
        }

        internal void Collapse(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row.IsExpanded);

            var nextIndex = row.RawIndex + 1;
            int count = NextIndexOf(row) - nextIndex;
            if (count == 0)
                return;

            RemoveRange(nextIndex, count);
            UpdateIndex(nextIndex);
            OnRowsChanged();
        }

        private int NextIndexOf(RowPresenter row)
        {
            Debug.Assert(IsRecursive && row != null && row.RawIndex >= 0);

            var depth = row.Depth;
            var result = row.RawIndex + 1;
            for (; result < _rows.Count; result++)
            {
                if (_rows[result].Depth <= depth)
                    break;
            }
            return result;
        }

        private void RemoveRange(int startIndex, int count)
        {
            Debug.Assert(count > 0);

            for (int i = 0; i < count; i++)
                _rows[startIndex + i].RawIndex = -1;

            _rows.RemoveRange(startIndex, count);
        }

        private void RemoveAt(int index)
        {
            _rows[index].RawIndex = -1;
            _rows.RemoveAt(index);
        }
    }
}
