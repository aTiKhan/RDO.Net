﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class LayoutXYManager : LayoutManager, IScrollHandler
    {
        private struct Span
        {
            public readonly double StartOffset;
            public readonly double EndOffset;

            public Span(double startOffset, double endOffset)
            {
                StartOffset = startOffset;
                EndOffset = endOffset;
            }

            public double Length
            {
                get { return EndOffset - StartOffset; }
            }
        }

        private struct RelativeOffset
        {
            public RelativeOffset(double value)
            {
                _value = value;
            }

            public RelativeOffset(int gridOffset, double fractionOffset)
            {
                Debug.Assert(gridOffset >= 0);
                Debug.Assert(fractionOffset >= 0 && fractionOffset < 1);
                _value = gridOffset + fractionOffset;
            }

            private readonly double _value;
            public int GridOffset
            {
                get { return (int)_value; }
            }

            public double FractionOffset
            {
                get { return _value - GridOffset; }
            }
        }

        private struct GridOffset
        {
            public static GridOffset Eof
            {
                get { return new GridOffset(); }
            }

            public static GridOffset New(GridTrack gridTrack)
            {
                Debug.Assert(!gridTrack.IsRepeat);
                return new GridOffset(gridTrack, -1);
            }

            public static GridOffset New(GridTrack gridTrack, int blockOrdinal)
            {
                Debug.Assert(gridTrack.IsRepeat);
                return new GridOffset(gridTrack, blockOrdinal);
            }

            private GridOffset(GridTrack gridTrack, int blockOrdinal)
            {
                GridTrack = gridTrack;
                _blockOrdinal = blockOrdinal;
            }

            public readonly GridTrack GridTrack;
            private readonly int _blockOrdinal;
            public int BlockOrdinal
            {
                get { return GridTrack == null || !GridTrack.IsRepeat ? -1 : _blockOrdinal; }
            }

            public bool IsEof
            {
                get { return GridTrack == null; }
            }
        }

        #region IScrollHandler

        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        private void InvalidateMeasure()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
        }

        private void InvalidateArrange()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateArrange();
        }

        public abstract double ViewportX { get; }

        public abstract double ViewportY { get; }

        protected double ViewportMain { get; private set; }

        protected double ViewportCross { get; private set; }

        private void RefreshViewport(double valueMain, double valueCross)
        {
            if (ViewportMain.IsClose(valueMain) && ViewportCross.IsClose(valueCross))
                return;
            ViewportMain = valueMain;
            ViewportCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ExtentY { get; }

        public abstract double ExtentX { get; }

        protected double ExtentMain { get; private set; }

        protected double ExtentCross { get; private set; }

        private void RefreshExtent(double valueMain, double valueCross)
        {
            if (ExtentMain.IsClose(valueMain) && ExtentCross.IsClose(valueCross))
                return;
            ExtentMain = valueMain;
            ExtentCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ScrollOffsetX { get; set; }

        public abstract double ScrollOffsetY { get; set; }

        private double _oldScrollOffsetMain;
        private double _scrollOffsetMain;
        protected double ScrollOffsetMain
        {
            get { return _scrollOffsetMain; }
            set { SetScrollOffsetMain(value, true); }
        }

        private void SetScrollOffsetMain(double value, bool invalidateMeasure)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private double _scrollOffsetCross;
        protected double ScrollOffsetCross
        {
            get { return _scrollOffsetCross; }
            set { SetScrollOffsetCross(value, true); }
        }

        private void SetScrollOffsetCross(double value, bool invalidateArrange)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = value;
            if (invalidateArrange)
                InvalidateArrange();
        }

        private void RefreshScollOffset(double valueMain, double valueCross)
        {
            _oldScrollOffsetMain = ScrollOffsetMain;
            if (ScrollOffsetMain.IsClose(valueMain) && ScrollOffsetCross.IsClose(valueCross))
                return;
            SetScrollOffsetMain(valueMain, false);
            SetScrollOffsetCross(valueCross, false);
            InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        #endregion

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to X or Y, and the overrides do not rely on completion of its constructor.")]
        protected LayoutXYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            VariantAutoLengthTracks = GridTracksMain.InitVariantAutoLengthTracks();
            ScrollStart = ScrollOrigin;
        }

        protected abstract IGridTrackCollection GridTracksMain { get; }
        protected abstract IGridTrackCollection GridTracksCross { get; }
        internal IReadOnlyList<GridTrack> VariantAutoLengthTracks { get; private set; }

        private RelativeOffset ScrollOrigin
        {
            get { return new RelativeOffset(FrozenHead); }
        }

        private RelativeOffset ScrollStart;

        private Vector ToVector(double valueMain, double valueCross)
        {
            return GridTracksMain.ToVector(valueMain, valueCross);
        }

        private Size ToSize(double valueMain, double valueCross)
        {
            var vector = ToVector(valueMain, valueCross);
            return new Size(vector.X, vector.Y);
        }

        private Point ToPoint(double valueMain, double valueCross)
        {
            var vector = ToVector(valueMain, valueCross);
            return new Point(vector.X, vector.Y);
        }

        private double TotalVariantAutoLength
        {
            get
            {
                Debug.Assert(BlockViews.Count > 0);
                return BlockViews.Last.EndMeasuredAutoLengthOffset - BlockViews.First.StartMeasuredAutoLengthOffset;
            }
        }

        private double AvgVariantAutoLength
        {
            get { return BlockViews.Count == 0 ? 0 : TotalVariantAutoLength / BlockViews.Count; }
        }

        private double FixBlockLength
        {
            get { return GridTracksMain.BlockEnd.EndOffset - GridTracksMain.BlockStart.StartOffset; }
        }

        private double AvgBlockLength
        {
            get { return FixBlockLength + AvgVariantAutoLength; }
        }

        private double TranslateRelativeOffset(RelativeOffset relativeOffset)
        {
            var gridOffset = relativeOffset.GridOffset;
            if (gridOffset >= MaxGridOffset)
                return GetSpan(MaxGridOffset - 1).EndOffset;
            else
            {
                var span = GetSpan(gridOffset);
                return span.StartOffset + span.Length * relativeOffset.FractionOffset;
            }
        }

        private RelativeOffset TranslateRelativeOffset(double offset)
        {
            // Binary search
            var min = 0;
            var max = MaxGridOffset - 1;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                var offsetSpan = GetSpan(mid);
                if (offset < offsetSpan.StartOffset)
                    max = mid - 1;
                else if (offset >= offsetSpan.EndOffset)
                    min = mid + 1;
                else
                    return new RelativeOffset(mid, (offset - offsetSpan.StartOffset) / offsetSpan.Length);
            }

            return new RelativeOffset(MaxGridOffset);
        }

        private Span GetSpan(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset < MaxGridOffset);
            return GetSpan(TranslateGridOffset(gridOffset));
        }

        private Span GetSpan(GridOffset gridOffset)
        {
            Debug.Assert(!gridOffset.IsEof);

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return new Span(gridTrack.StartOffset, gridTrack.EndOffset);

            if (gridTrack.IsRepeat)
                return GetGridSpan(gridTrack, gridOffset.BlockOrdinal);

            Debug.Assert(gridTrack.IsTail);
            var delta = MaxBlockCount * AvgBlockLength;
            if (MaxBlockCount > 0)
                delta -= FixBlockLength;    // minus duplicated FixBlockLength
            return new Span(gridTrack.StartOffset + delta, gridTrack.EndOffset + delta);
        }

        private Span GetGridSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(blockOrdinal >= 0);

            var relativeSpan = GetRelativeSpan(gridTrack, blockOrdinal);
            var startOffset = GridTracksMain[MaxFrozenHead].StartOffset + GetBlocksLength(blockOrdinal);
            return new Span(startOffset + relativeSpan.StartOffset, startOffset + relativeSpan.EndOffset);
        }

        private Span GetRelativeSpan(GridTrack gridTrack, int blockOrdinal)
        {
            var startTrack = GridTracksMain.BlockStart;

            var startOffset = gridTrack.StartOffset - startTrack.StartOffset;
            var endOffset = gridTrack.EndOffset - startTrack.StartOffset;

            var variantAutoLengthTrack = LastVariantAutoLengthTrack(gridTrack);
            if (variantAutoLengthTrack != null)
            {
                var variantLengthSpan = GetVariantLengthSpan(variantAutoLengthTrack, blockOrdinal);
                startOffset += variantLengthSpan.StartOffset;
                endOffset += variantLengthSpan.EndOffset;
            }
            return new Span(startOffset, endOffset);
        }

        private Span GetVariantLengthSpan(GridTrack gridTrack, int blockOrdinal)
        {
            Debug.Assert(gridTrack.IsVariantAutoLength);

            double startOffset, endOffset;

            var blockView = BlockViews.GetBlockView(blockOrdinal);
            if (blockView != null)
            {
                startOffset = blockView.GetMeasuredAutoLengthStartOffset(gridTrack);
                endOffset = blockView.GetMeasuredAutoLengthEndOffset(gridTrack);
            }
            else
            {
                endOffset = 0;
                for (int i = 0; i <= gridTrack.VariantAutoLengthIndex; i++)
                    endOffset += VariantAutoLengthTracks[i].AvgVariantAutoLength;
                startOffset = endOffset - gridTrack.AvgVariantAutoLength;
            }

            return new Span(startOffset, endOffset);
        }

        private GridTrack LastVariantAutoLengthTrack(GridTrack gridTrack)
        {
            GridTrack result = null;
            foreach (var variantAuotLengthTrack in VariantAutoLengthTracks)
            {
                if (variantAuotLengthTrack.Ordinal > gridTrack.Ordinal)
                    break;

                result = variantAuotLengthTrack;
            }
            return result;
        }

        private double GetBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= MaxBlockCount);
            if (count == 0)
                return 0;

            var unrealized = BlockViews.Count == 0 ? 0 : BlockViews.First.Ordinal;
            if (count <= unrealized)
                return count * AvgBlockLength;

            var realized = BlockViews.Count == 0 ? 0 : BlockViews.Last.Ordinal - BlockViews.First.Ordinal + 1;
            if (count <= unrealized + realized)
                return unrealized * AvgBlockLength + GetRealizedBlocksLength(count - unrealized);

            return GetRealizedBlocksLength(realized) + (count - realized) * AvgBlockLength;
        }

        private double GetRealizedBlocksLength(int count)
        {
            Debug.Assert(count >= 0 && count <= BlockViews.Count);
            return count == 0 ? 0 : count * FixBlockLength + BlockViews[count - 1].EndMeasuredAutoLengthOffset;
        }

        private GridOffset TranslateGridOffset(int gridOffset)
        {
            Debug.Assert(gridOffset >= 0 && gridOffset <= MaxGridOffset);
            if (gridOffset < MaxFrozenHead)
                return GridOffset.New(GridTracksMain[gridOffset]);

            gridOffset -= MaxFrozenHead;
            var totalBlockGridTracks = TotalBlockGridTracks;
            if (gridOffset < totalBlockGridTracks)
                return GridOffset.New(GridTracksMain[MaxFrozenHead + gridOffset % BlockGridTracks], gridOffset / BlockGridTracks);

            gridOffset -= totalBlockGridTracks;
            Debug.Assert(gridOffset < MaxFrozenTail);
            return GridOffset.New(GridTracksMain[MaxFrozenHead + BlockGridTracks + gridOffset]);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int FrozenHead
        {
            get { return GridTracksMain.FrozenHead; }
        }

        private int MaxFrozenHead
        {
            get { return GridTracksMain.MaxFrozenHead; }
        }

        private int BlockGridTracks
        {
            get { return GridTracksMain.BlockEnd.Ordinal - GridTracksMain.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracks
        {
            get { return MaxBlockCount * BlockGridTracks; }
        }

        private int MaxFrozenTail
        {
            get { return GridTracksMain.MaxFrozenTail; }
        }

        private int MaxGridOffset
        {
            get { return MaxFrozenHead + TotalBlockGridTracks + MaxFrozenTail; }
        }

        private double MaxOffsetMain
        {
            get { return GetSpan(MaxGridOffset - 1).EndOffset; }
        }

        private double MaxOffsetCross
        {
            get { return GridTracksCross.GetMeasuredLength(Template.Range()); }
        }

        private Vector BlockDimensionVector
        {
            get { return GridTracksMain.BlockDimensionVector; }
        }

        protected sealed override double GetMeasuredLength(BlockView blockView, GridTrack gridTrack)
        {
            return blockView != null && gridTrack.IsVariantAutoLength ? blockView.GetMeasuredAutoLength(gridTrack) : base.GetMeasuredLength(blockView, gridTrack);
        }

        protected override bool SetMeasuredAutoLength(BlockView blockView, GridTrack gridTrack, double value)
        {
            if (blockView != null && gridTrack.IsVariantAutoLength)
            {
                blockView.SetMeasuredAutoLength(gridTrack, value);
                return false;
            }
            else
                return base.SetMeasuredAutoLength(blockView, gridTrack, value);
        }

        private bool _isVariantAutoLengthsValid = true;
        internal void InvalidateVariantAutoLengths()
        {
            _isVariantAutoLengthsValid = false;
        }

        internal void RefreshVariantAutoLengths()
        {
            if (_isVariantAutoLengthsValid)
                return;

            _isVariantAutoLengthsValid = true; // Avoid re-entrance
            for (int i = 1; i < BlockViews.Count; i++)
                BlockViews[i].StartMeasuredAutoLengthOffset = BlockViews[i - 1].EndMeasuredAutoLengthOffset;

            foreach (var gridTrack in VariantAutoLengthTracks)
            {
                double totalVariantAutoLength = 0;
                for (int i = 0; i < BlockViews.Count; i++)
                    totalVariantAutoLength += BlockViews[i].GetMeasuredAutoLength(gridTrack);
                gridTrack.SetAvgVariantAutoLength(BlockViews.Count == 0 ? 0 : totalVariantAutoLength / BlockViews.Count);
            }
        }

        protected override void OnSetState(DataPresenterState dataPresenterState)
        {
            base.OnSetState(dataPresenterState);
            if (dataPresenterState == DataPresenterState.Rows)
            {
                BlockViews.VirtualizeAll();
                InvalidateVariantAutoLengths();
            }
        }

        internal override Size Measure(Size availableSize)
        {
            ValidateScrollStart();
            return base.Measure(availableSize);
        }

        private void ValidateScrollStart()
        {

        }

        protected sealed override void PrepareMeasureBlocks()
        {

            RefreshScrollInfo();
        }

        private void RefreshScrollInfo()
        {
            RefreshViewport();
            RefreshExtent();  // Exec order matters: RefreshExtent relies on RefreshViewport
            RefreshScrollOffset();  // Exec order matters: RefreshScrollOffset relies on RefreshViewport and RefreshExtent
        }

        private void RefreshExtent()
        {
            var valueMain = MaxOffsetMain;
            if (valueMain < ViewportMain)
                valueMain = ViewportMain;
            var valueCross = MaxOffsetCross;
            if (valueCross < ViewportCross)
                valueCross = ViewportCross;
            RefreshExtent(valueMain, valueCross);
        }

        private void RefreshViewport()
        {
            var valueMain = GridTracksMain.SizeToContent ? MaxOffsetMain : GridTracksMain.AvailableLength;
            var valueCross = GridTracksCross.SizeToContent ? MaxOffsetCross : GridTracksCross.AvailableLength;
            RefreshViewport(valueMain, valueCross);
        }

        private void RefreshScrollOffset()
        {
            var valueMain = TranslateRelativeOffset(ScrollStart) - TranslateRelativeOffset(ScrollOrigin);
            var valueCross = CoerceScrollOffsetCross();
            RefreshScollOffset(valueMain, valueCross);
        }

        private double CoerceScrollOffsetCross()
        {
            var result = ScrollOffsetCross;
            if (result < 0)
                result = 0;
            if (result > ExtentCross - ViewportCross)
                result = ExtentCross - ViewportCross;
            return result;
        }

        protected sealed override Size GetMeasuredSize(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var mainGridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startGridOffset = GetStartGridOffset(mainGridSpan.StartTrack);
            var endGridOffset = GetEndGridOffset(mainGridSpan.EndTrack);
            var valueMain = startGridOffset == endGridOffset ? GetSpan(startGridOffset).Length
                : GetSpan(endGridOffset).EndOffset - GetSpan(startGridOffset).StartOffset;
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);

            var result = ToSize(valueMain, valueCross);
            if (!scalarItem.IsMultidimensional && BlockDimensions > 1)
            {
                var delta = BlockDimensionVector * (BlockDimensions - 1);
                result = new Size(result.Width + delta.X, result.Height + delta.Y);
            }
            return result;
        }

        protected override Size GetMeasuredSize(BlockView blockView, GridRange gridRange)
        {
            Debug.Assert(!gridRange.IsEmpty && Template.BlockRange.Contains(gridRange));

            var valueMain = GetMeasuredLength(blockView, gridRange);
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);
            return ToSize(valueMain, valueCross);
        }

        private double GetMeasuredLength(BlockView blockView, GridRange gridRange)
        {
            Debug.Assert(!gridRange.IsEmpty && Template.BlockRange.Contains(gridRange));

            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            Span startTrackSpan, endTrackSpan;
            if (gridSpan.StartTrack == gridSpan.EndTrack)
                startTrackSpan = endTrackSpan = GetRelativeSpan(gridSpan.StartTrack, blockView.Ordinal);
            else
            {
                startTrackSpan = GetRelativeSpan(gridSpan.StartTrack, blockView.Ordinal);
                endTrackSpan = GetRelativeSpan(gridSpan.EndTrack, blockView.Ordinal);
            }
            return endTrackSpan.EndOffset - startTrackSpan.StartOffset;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            var gridRange = scalarItem.GridRange;
            var valueMain = GetSpan(GetStartGridOffset(GridTracksMain.GetGridSpan(gridRange).StartTrack)).StartOffset;
            var valueCross = GridTracksCross.GetGridSpan(gridRange).StartTrack.StartOffset;
            var result = ToPoint(valueMain, valueCross);
            if (blockDimension > 0)
                result += (blockDimension - 1) * BlockDimensionVector;
            return result;
        }

        private int GetStartGridOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Orientation == Template.Orientation);

            var result = gridTrack.Ordinal;
            if (result >= MaxFrozenHead + BlockGridTracks)
                result += MaxBlockCount * BlockGridTracks;
            return result;
        }

        private int GetEndGridOffset(GridTrack gridTrack)
        {
            Debug.Assert(gridTrack.Orientation == Template.Orientation);

            var result = gridTrack.Ordinal;

            if (result >= MaxFrozenHead + BlockGridTracks)
                result += MaxBlockCount * BlockGridTracks;
            else if (result >= MaxFrozenHead && MaxBlockCount > 1)
                result += (MaxBlockCount - 1) * BlockGridTracks;

            return result;
        }

        protected override Point GetBlockViewLocation(BlockView blockView)
        {
            throw new NotImplementedException();
        }

        protected override Point GetBlockItemLocation(BlockView blockView, BlockItem blockItem)
        {
            throw new NotImplementedException();
        }

        protected override Point GetRowViewLocation(BlockView blockView, int blockDimension)
        {
            throw new NotImplementedException();
        }

        protected override Point GetRowItemLocation(BlockView blockView, RowItem rowItem)
        {
            throw new NotImplementedException();
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportX, ViewportY); }
        }
    }
}
