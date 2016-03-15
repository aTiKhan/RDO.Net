﻿using System;
using System.Diagnostics;

namespace DevZest.Data.Windows
{
    public sealed class TemplateBuilder : IDisposable
    {
        internal TemplateBuilder(Template template)
        {
            Debug.Assert(template != null);
            Template = template;
        }

        public void Dispose()
        {
            Template = null;
        }

        internal Template Template { get; private set; }

        public TemplateBuilder AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public TemplateBuilder AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public TemplateBuilder AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public TemplateBuilder AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public TemplateBuilder WithOrientation(RepeatOrientation value)
        {
            Template.RepeatOrientation = value;
            return this;
        }

        public GridRangeConfig Range(int column, int row)
        {
            return new GridRangeConfig(this, Template.Range(column, row));
        }

        public GridRangeConfig Range(int left, int top, int right, int bottom)
        {
            return new GridRangeConfig(this, Template.Range(left, top, right, bottom));
        }

        public TemplateBuilder WithPinnedLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public TemplateBuilder WithPinnedTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public TemplateBuilder WithPinnedRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public TemplateBuilder WithPinnedBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public TemplateBuilder Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public TemplateBuilder WithEofVisible(bool value)
        {
            Template.IsEofVisible = value;
            return this;
        }

        public TemplateBuilder WithEmptySetVisible(bool value)
        {
            Template.IsEmptySetVisible = value;
            return this;
        }
    }
}
