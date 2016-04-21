﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    public abstract class GridTrack
    {
        internal GridTrack(Template owner, int ordinal, GridLengthParser.Result result)
        {
            Owner = owner;
            Ordinal = ordinal;
            Length = result.Length;
            MinLength = result.MinLength;
            MaxLength = result.MaxLength;
            VariantAutoLengthIndex = -1;
        }

        internal Template Owner { get; private set; }

        internal int Ordinal { get; private set; }

        public GridLength Length { get; private set; }

        public double MinLength { get; private set; }

        public double MaxLength { get; private set; }

        internal bool IsAutoLength(bool sizeToContent)
        {
            return Length.IsAuto || (Length.IsStar && sizeToContent);
        }

        internal bool IsStarLength(bool sizeToContent)
        {
            return Length.IsStar && !sizeToContent;
        }

        internal int AutoLengthIndex { get; set; }

        internal double MeasuredLength { get; set; }

        internal double StartOffset { get; set; }

        internal double EndOffset
        {
            get { return StartOffset + MeasuredLength; }
        }

        internal int VariantAutoLengthIndex { get; set; }

        internal bool IsVariantAutoLength
        {
            get { return VariantAutoLengthIndex >= 0; }
        }

        public abstract Orientation Orientation { get; }
    }
}
