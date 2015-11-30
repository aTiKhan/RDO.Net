﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Windows
{
    public sealed class GridTemplate
    {
        internal GridTemplate(Model model)
        {
            Model = model;
            GridRows = new GridDefinitionCollection<GridRow>();
            GridColumns = new GridDefinitionCollection<GridColumn>();
            ScalarItems = new GridItemCollection(this);
            ListItems = new GridItemCollection(this);
            ChildTemplates = new List<GridTemplate>();
        }

        internal void Seal()
        {
            _isSealed = true;
        }

        internal int ChildOrdinal { get; private set; }

        bool _isSealed = false;
        public bool IsSealed
        {
            get { return _isSealed; }
        }
        private void VerifyIsSealed()
        {
            if (_isSealed)
                throw new InvalidOperationException(Strings.GridTemplate_VerifyIsSealed);
        }

        public Model Model { get; private set; }

        private DataRowOrientation _orientation = DataRowOrientation.Y;
        public DataRowOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                VerifyIsSealed();
                VerifyGridColumnWidth(value, 0, GridColumns.Count - 1, nameof(value));
                VerifyGridRowHeight(value, 0, GridRows.Count - 1, nameof(value));
                _orientation = value;
            }
        }

        public GridTemplate SetOrientation(DataRowOrientation value)
        {
            Orientation = value;
            return this;
        }

        private ScrollMode? _scrollMode;
        public ScrollMode ScrollMode
        {
            get { return _scrollMode.HasValue ? _scrollMode.GetValueOrDefault() : DefaultScrollMode; }
            set
            {
                VerifyIsSealed();
                _scrollMode = value;
            }
        }

        private ScrollMode DefaultScrollMode
        {
            get { return Orientation == DataRowOrientation.Z ? ScrollMode.None : ScrollMode.Virtualizing; }
        }

        public GridTemplate SetScrollMode(ScrollMode value)
        {
            ScrollMode = value;
            return this;
        }

        private bool _canAddNew;
        public bool CanAddNew
        {
            get { return _canAddNew; }
            set
            {
                VerifyIsSealed();
                _canAddNew = value;
            }
        }

        public GridTemplate SetCanAddNew(bool value)
        {
            CanAddNew = value;
            return this;
        }

        private GridRange? _dataRowRange;
        public GridRange DataRowRange
        {
            get { return _dataRowRange.HasValue ? _dataRowRange.GetValueOrDefault() : GetGridRangeAll(); }
            set
            {
                VerifyIsSealed();
                if (!GetGridRangeAll().Contains(value) || !value.Contains(CalculatedDataRowRange))
                    throw new ArgumentOutOfRangeException(nameof(value));

                _dataRowRange = value;
            }
        }

        private GridRange CalculatedDataRowRange
        {
            get { return ListItems.Range; }
        }

        public GridTemplate SetDataRowRange(GridRange value)
        {
            DataRowRange = value;
            return this;
        }

        private int _frozenCount;
        public int FrozenCount
        {
            get { return _frozenCount; }
            set
            {
                VerifyIsSealed();
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                _frozenCount = value;
            }
        }

        public GridTemplate SetFrozenCount(int value)
        {
            FrozenCount = value;
            return this;
        }

        public GridDefinitionCollection<GridRow> GridRows { get; private set; }

        public GridDefinitionCollection<GridColumn> GridColumns { get; private set; }

        public GridItemCollection ScalarItems { get; private set; }

        public GridItemCollection ListItems { get; private set; }

        internal List<GridTemplate> ChildTemplates { get; private set; }

        public GridTemplate AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            foreach (var height in heights)
                AddGridRow(height);
            return this;
        }

        public GridTemplate AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            foreach (var width in widths)
                AddGridColumn(width);
            return this;
        }

        private int AddGridColumn(string width)
        {
            VerifyIsSealed();
            GridColumns.Add(new GridColumn(this, GridColumns.Count, GridLengthParser.Parse(width)));
            var result = GridColumns.Count - 1;
            VerifyGridColumnWidth(Orientation, result, result, nameof(width));
            return result;
        }

        public GridTemplate AddGridColumn(string width, out int index)
        {
            index = AddGridColumn(width);
            return this;
        }

        private int AddGridRow(string height)
        {
            VerifyIsSealed();
            GridRows.Add(new GridRow(this, GridRows.Count, GridLengthParser.Parse(height)));
            var result = GridRows.Count - 1;
            VerifyGridRowHeight(Orientation, result, result, nameof(height));
            return result;
        }

        public GridTemplate AddGridRow(string height, out int index)
        {
            index = AddGridRow(height);
            return this;
        }

        private void VerifyGridRowHeight(DataRowOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridRowHeight(GridRows[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridRowHeightOrientation(i, GridRows[i].Height, orientation), paramName);
            }
        }

        private static bool IsValidGridRowHeight(GridRow gridRow, DataRowOrientation orentation)
        {
            var height = gridRow.Height;

            if (height.IsAbsolute)
                return true;

            if (height.IsStar)
                return orentation != DataRowOrientation.Y && orentation != DataRowOrientation.YX && orentation != DataRowOrientation.XY;

            Debug.Assert(height.IsAuto);
            return orentation != DataRowOrientation.YX;
        }

        private void VerifyGridColumnWidth(DataRowOrientation orientation, int startIndex, int endIndex, string paramName)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!IsValidGridColumnWidth(GridColumns[i], orientation))
                    throw new ArgumentException(Strings.GridTemplate_InvalidGridColumnWidthOrientation(i, GridColumns[i].Width, orientation), paramName);
            }
        }

        private static bool IsValidGridColumnWidth(GridColumn gridColumn, DataRowOrientation orentation)
        {
            var width = gridColumn.Width;

            if (width.IsAbsolute)
                return true;

            if (width.IsStar)
                return orentation != DataRowOrientation.X && orentation != DataRowOrientation.YX && orentation != DataRowOrientation.XY;

            Debug.Assert(width.IsAuto);
            return orentation != DataRowOrientation.XY;
        }

        internal GridTemplate AddScalarItem<T>(GridRange gridRange, ScalarGridItem<T> scalarItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, scalarItem, nameof(scalarItem));
            ScalarItems.Add(scalarItem, gridRange);
            return this;
        }

        internal GridTemplate AddListItem<T>(GridRange gridRange, ListGridItem<T> listItem)
            where T : UIElement, new()
        {
            VerifyAddItem(gridRange, listItem, nameof(listItem));
            ListItems.Add(listItem, gridRange);
            return this;
        }

        internal GridTemplate AddChildItem<T>(GridRange gridRange, ChildGridItem<T> childItem)
            where T : DataSetGrid, new()
        {
            VerifyAddItem(gridRange, childItem, nameof(childItem));
            ListItems.Add(childItem, gridRange);
            var childTemplate = childItem.Template;
            childTemplate.ChildOrdinal = ChildTemplates.Count;
            ChildTemplates.Add(childTemplate);
            return this;
        }

        private void VerifyAddItem(GridRange gridRange, GridItem gridItem, string paramGridItemName)
        {
            VerifyIsSealed();
            if (!GetGridRangeAll().Contains(gridRange))
                throw new ArgumentOutOfRangeException(nameof(gridRange));
            if (gridItem == null)
                throw new ArgumentNullException(paramGridItemName);
            if (gridItem.Owner != null || (gridItem.ParentModel != null && gridItem.ParentModel != Model))
                throw new ArgumentException(Strings.GridTemplate_InvalidGridItem, paramGridItemName);
        }


        private void VerifyGridColumn(int index, string paramName)
        {
            if (index < 0 || index >= GridColumns.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private void VerifyGridRow(int index, string paramName)
        {
            if (index < 0 || index >= GridRows.Count)
                throw new ArgumentOutOfRangeException(paramName);
        }

        private GridRange GetGridRangeAll()
        {
            if (GridColumns.Count == 0 || GridRows.Count == 0)
                return new GridRange();

            return new GridRange(GridColumns[0], GridRows[0], GridColumns[GridColumns.Count - 1], GridRows[GridRows.Count - 1]);
        }

        public GridRange Range(int column, int row)
        {
            VerifyGridColumn(column, nameof(column));
            VerifyGridRow(row, nameof(row));
            return new GridRange(GridColumns[column], GridRows[row]);
        }

        public GridRange Range(int left, int top, int right, int bottom)
        {
            VerifyGridColumn(left, nameof(left));
            VerifyGridRow(top, nameof(top));
            VerifyGridColumn(right, nameof(right));
            VerifyGridRow(bottom, nameof(bottom));
            if (right < left)
                throw new ArgumentOutOfRangeException(nameof(right));
            if (bottom < top)
                throw new ArgumentOutOfRangeException(nameof(bottom));
            return new GridRange(GridColumns[left], GridRows[top], GridColumns[right], GridRows[bottom]);
        }

        internal void DefaultInitialize()
        {
            var columns = Model.GetColumns();

            this.AddGridColumns(columns.Select(x => "Auto").ToArray())
                .AddGridRows("Auto", "Auto")
                .SetDataRowRange(Range(0, 1, columns.Count - 1, 1));

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                //this.AddScalarItem(this[i, 0], column)
                //    .AddSetItem(this[i, 1], column.TextBlock());
            }
        }
    }
}
