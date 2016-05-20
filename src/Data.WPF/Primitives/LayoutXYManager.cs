﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract partial class LayoutXYManager : LayoutManager, IScrollHandler
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "Derived classes are limited to class LayoutXManager/LayoutYManager, and the overrides do not rely on completion of its constructor.")]
        protected LayoutXYManager(Template template, DataSet dataSet)
            : base(template, dataSet)
        {
            _scrollStartMain = ScrollOriginMain;
        }

        internal abstract IGridTrackCollection GridTracksMain { get; }
        internal abstract IGridTrackCollection GridTracksCross { get; }

        internal GridSpan VariantByBlockGridSpan
        {
            get { return GridTracksMain.VariantByBlock ? GridTracksMain.GetGridSpan(Template.RowRange) : new GridSpan(); }
        }

        protected override void DisposeRow(RowPresenter row)
        {
            var gridSpan = VariantByBlockGridSpan;
            for (int i = 0; i < gridSpan.Count; i++)
                gridSpan[i].ClearAvailableLength(row);
            base.DisposeRow(row);
        }

        private bool _isBlockLengthsValid = true;
        internal void InvalidateBlockLengths()
        {
            _isBlockLengthsValid = false;
        }

        internal void RefreshBlockLengths()
        {
            if (_isBlockLengthsValid)
                return;

            _isBlockLengthsValid = true; // Avoid re-entrance
            for (int i = 1; i < BlockViews.Count; i++)
                BlockViews[i].StartOffset = BlockViews[i - 1].EndOffset;

            var gridSpan = VariantByBlockGridSpan;
            if (gridSpan.IsEmpty)
                return;

            for (int i = 0; i < gridSpan.Count; i++)
            {
                var gridTrack = gridSpan[i];
                double totalLength = 0;
                for (int j = 0; j < BlockViews.Count; j++)
                    totalLength += BlockViews[j].GetMeasuredLength(gridTrack);
                gridTrack.VariantByBlockAvgLength = BlockViews.Count == 0 ? 1 : totalLength / BlockViews.Count;
            }

            for (int i = 1; i < gridSpan.Count; i++)
                gridSpan[i].VariantByBlockStartOffset = gridSpan[i - 1].VariantByBlockEndOffset;
        }

        private bool _isBlocksDirty;
        private void InvalidateBlocks()
        {
            if (_isBlocksDirty)
                return;

            for (int i = 0; i < BlockViews.Count; i++)
                BlockViews[i].ClearElements();
            _isBlocksDirty = true;

            InvalidateMeasure();
        }

        private void InitBlocks()
        {
            BlockViews.VirtualizeAll();

            var initialBlockOrdinal = GetInitialBlockOrdinal();
            if (initialBlockOrdinal >= 0)
            {
                BlockViews.RealizeFirst(initialBlockOrdinal);
                BlockViews[0].Measure(Size.Empty);
            }
            _isBlocksDirty = false;
        }

        private int GetInitialBlockOrdinal()
        {
            if (MaxBlockCount == 0)
                return -1;

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
                return MaxBlockCount - 1;

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsRepeat)
                return gridOffset.BlockOrdinal;
            else
                return MaxBlockCount - 1;
        }

        private LogicalOffset ScrollOriginMain
        {
            get { return new LogicalOffset(FrozenHeadMain); }
        }

        private LogicalOffset _scrollStartMain;
        private double ScrollStartMain
        {
            get { return GetOffset(_scrollStartMain); }
        }

        private void AdjustScrollStartMain(double delta)
        {
            _scrollStartMain = GetLogicalOffset(ScrollStartMain + delta);
            Debug.Assert(_scrollStartMain.Value >= ScrollOriginMain.Value);
        }

        private double ScrollOriginCross
        {
            get { return GridTracksCross[GridTracksCross.FrozenHead].StartOffset; }
        }

        private double ScrollStartCross
        {
            get { return ScrollOriginCross + ScrollOffsetCross; }
        }

        private double DeltaScrollOffset
        {
            get { return _scrollOffsetMain - _oldScrollOffsetMain; }
        }

        private void ClearDeltaScrollOffset()
        {
            _oldScrollOffsetMain = _scrollOffsetMain;
        }

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

        private Thickness ToThickness(Clip clipMain, Clip clipCross)
        {
            var vectorHead = ToVector(clipMain.Head, clipCross.Head);
            var vectorTail = ToVector(clipMain.Tail, clipCross.Tail);
            return new Thickness(vectorHead.X, vectorHead.Y, vectorTail.X, vectorTail.Y);
        }

        private int MaxBlockCount
        {
            get { return BlockViews.MaxBlockCount; }
        }

        private int FrozenHeadMain
        {
            get { return GridTracksMain.FrozenHead; }
        }

        private int FrozenHeadCross
        {
            get { return GridTracksCross.FrozenHead; }
        }

        private int MaxFrozenHeadMain
        {
            get { return GridTracksMain.MaxFrozenHead; }
        }

        private int BlockGridTracksMain
        {
            get { return GridTracksMain.BlockEnd.Ordinal - GridTracksMain.BlockStart.Ordinal + 1; }
        }

        private int TotalBlockGridTracksMain
        {
            get { return MaxBlockCount * BlockGridTracksMain; }
        }

        private int FrozenTailMain
        {
            get { return GridTracksMain.FrozenTail; }
        }

        private int FrozenTailCross
        {
            get { return GridTracksCross.FrozenTail; }
        }

        private int MaxFrozenTailMain
        {
            get { return GridTracksMain.MaxFrozenTail; }
        }

        private int MaxGridOffsetMain
        {
            get { return MaxFrozenHeadMain + TotalBlockGridTracksMain + MaxFrozenTailMain; }
        }

        private double MaxOffsetMain
        {
            get { return GetGridOffset(MaxGridOffsetMain - 1).Span.EndOffset; }
        }

        private double MaxOffsetCross
        {
            get
            {
                var result = GridTracksCross.GetMeasuredLength(Template.Range());
                if (BlockDimensions > 1)
                    result += BlockDimensionLength * (BlockDimensions - 1);
                return result;
            }
        }

        private double BlockDimensionLength
        {
            get { return GridTracksCross.GetMeasuredLength(Template.RowRange); }
        }

        private Vector BlockDimensionVector
        {
            get { return ToVector(0, BlockDimensionLength); }
        }

        protected override void OnSetState(DataPresenterState dataPresenterState)
        {
            base.OnSetState(dataPresenterState);
            if (dataPresenterState == DataPresenterState.Rows)
                InvalidateBlocks();
        }

        internal override Size Measure(Size availableSize)
        {
            InitScroll();
            return base.Measure(availableSize);
        }

        private void InitScroll()
        {
            if (DeltaScrollOffset == 0 || (DeltaScrollOffset < 0 && Math.Abs(DeltaScrollOffset) <= ViewportMain))
                return;

            AdjustScrollStartMain(DeltaScrollOffset);
            ClearDeltaScrollOffset();
        }

        protected sealed override void PrepareMeasureBlocks()
        {
            InitBlocks();

            if (DeltaScrollOffset < 0 || _scrollStartMain.GridOffset >= MaxGridOffsetMain)
                MeasureBackward(-DeltaScrollOffset);
            else
                MeasureForward(GridTracksMain.AvailableLength);
            FillGap();
            RefreshScrollInfo();
        }

        private double HeadEnd
        {
            get { return GridTracksMain[MaxFrozenHeadMain].StartOffset; }
        }

        private double TailStart
        {
            get { return MaxFrozenTailMain == 0 ? MaxOffsetMain : GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.StartOffset; }
        }

        private double FrozenHeadLengthMain
        {
            get { return GridTracksMain[FrozenHeadMain].StartOffset; }
        }

        private double FrozenHeadLengthCross
        {
            get { return GridTracksCross[FrozenHeadCross].StartOffset; }
        }

        private double FrozenTailLengthMain
        {
            get
            {
                if (FrozenTailMain == 0)
                    return 0;

                var totalTracks = GridTracksMain.Count;
                var endOffset = GridTracksMain[totalTracks - 1].EndOffset;
                var startOffset = GridTracksMain[totalTracks - FrozenTailMain].StartOffset;
                return endOffset - startOffset;
            }
        }

        private double FrozenTailLengthCross
        {
            get { return GetTailLengthCross(FrozenTailCross); }
            //{
            //    if (FrozenTailCross == 0)
            //        return 0;

            //    var totalTracks = GridTracksCross.Count;
            //    var endOffset = GridTracksCross[totalTracks - 1].EndOffset;
            //    var startOffset = GridTracksCross[totalTracks - FrozenTailCross].StartOffset;
            //    return endOffset - startOffset;
            //}
        }

        private double GetTailLengthCross(int tracksCount)
        {
            if (tracksCount == 0)
                return 0;

            var totalTracks = GridTracksCross.Count;
            var endOffset = GridTracksCross[totalTracks - 1].EndOffset;
            var startOffset = GridTracksCross[totalTracks - tracksCount].StartOffset;
            return endOffset - startOffset;
        }

        private double TailLength
        {
            get { return MaxFrozenTailMain == 0 ? 0 : MaxOffsetMain - GetGridOffset(MaxGridOffsetMain - MaxFrozenTailMain).Span.StartOffset; }
        }

        private double Gap
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var scrollable = availableLength - (FrozenHeadLengthMain + TailLength);
                var blockEndOffset = BlockViews.Count == 0 ? GridTracksMain[MaxFrozenHeadMain].StartOffset : GetEndOffset(BlockViews[BlockViews.Count - 1]);
                return scrollable - (blockEndOffset - ScrollStartMain);
            }
        }

        private void FillGap()
        {
            var gap = Gap;
            gap -= RealizeForward(gap);
            if (gap > 0)
                MeasureBackward(gap);
        }

        private void MeasureBackward(double availableLength)
        {
            Debug.Assert(availableLength >= 0);

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            if (gridOffset.IsEof)
            {
                MeasureBackwardEof(availableLength);
                return;
            }
                

            var gridTrack = gridOffset.GridTrack;
            if (gridTrack.IsTail)
                MeasureBackwardTail(availableLength);
            else if (gridTrack.IsRepeat)
                MeasureBackwardRepeat(availableLength);
            else
            {
                Debug.Assert(gridTrack.IsHead);
                MeasureBackwardHead(availableLength);
            }
        }

        private void MeasureBackwardEof(double availableLength)
        {
            if (MaxFrozenTailMain > 0)
                MeasureBackwardTail(availableLength);
            else if (MaxBlockCount > 0)
                MeasureBackwardRepeat(availableLength);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private void MeasureBackwardTail(double availableLength)
        {
            var measuredLength = Math.Min(availableLength, ScrollStartMain - TailStart);
            Debug.Assert(measuredLength >= 0);
            if (measuredLength > 0)
            {
                AdjustScrollStartMain(-measuredLength);
                availableLength -= measuredLength;
            }
            if (availableLength == 0)
                return;

            if (MaxBlockCount > 0)
                MeasureBackwardRepeat(availableLength);
            else if (MaxFrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private void MeasureBackwardRepeat(double availableLength)
        {
            Debug.Assert(availableLength > 0);

            var block = BlockViews[0];
            var scrollLength = Math.Min(availableLength, ScrollStartMain - GetStartOffset(block));
            if (scrollLength > 0)
            {
                AdjustScrollStartMain(-scrollLength);
                availableLength -= scrollLength;
            }
            availableLength -= RealizeBackward(availableLength);
            if (availableLength > 0 && FrozenHeadMain > 0)
                MeasureBackwardHead(availableLength);
        }

        private double RealizeBackward(double availableLength)
        {
            if (BlockViews.Count == 0)
                return availableLength;

            Debug.Assert(BlockViews.Count > 0);

            for (int blockOrdinal = BlockViews.First.Ordinal - 1; blockOrdinal >= 0 && availableLength > 0; blockOrdinal--)
            {
                BlockViews.RealizePrev();
                var block = BlockViews.First;
                block.Measure(Size.Empty);
                var measuredLength = Math.Min(availableLength, GetBlockLengthMain(block));
                AdjustScrollStartMain(-measuredLength);
                availableLength -= measuredLength;
            }
            return availableLength;
        }

        private void MeasureBackwardHead(double availableLength)
        {
            Debug.Assert(availableLength >= 0);
            var measuredLength = Math.Min(availableLength, ScrollOffsetMain);
            if (measuredLength > 0)
               AdjustScrollStartMain(-measuredLength);
        }

        private void MeasureForward(double availableLength)
        {
            availableLength -= Math.Max(FrozenHeadLengthMain, HeadEnd - ScrollOffsetMain);
            if (availableLength <= 0)
                return;

            var gridOffset = GetGridOffset(_scrollStartMain.GridOffset);
            Debug.Assert(!gridOffset.IsEof);
            var gridTrack = gridOffset.GridTrack;
            var fraction = _scrollStartMain.FractionOffset;
            if (gridTrack.IsHead)
                MeasureForwardHead(gridTrack, fraction, availableLength);
            else if (gridTrack.IsRepeat)
                MeasureForwardRepeat(gridOffset, fraction, availableLength);
        }

        private void MeasureForwardHead(GridTrack gridTrack, double fraction, double availableLength)
        {
            Debug.Assert(gridTrack.IsHead);
            var measuredLength = HeadEnd - (gridTrack.StartOffset + gridTrack.MeasuredLength * fraction);
            Debug.Assert(measuredLength > 0);
            if (MaxBlockCount > 0)
                MeasureForwardRepeat(new GridOffset(GridTracksMain.BlockStart, 0), 0, availableLength - measuredLength);
        }

        private void MeasureForwardRepeat(GridOffset gridOffset, double fraction, double availableLength)
        {
            Debug.Assert(BlockViews.Count == 1);
            if (FrozenTailMain > 0)
                availableLength -= FrozenTailLengthMain;

            var gridTrack = gridOffset.GridTrack;
            Debug.Assert(gridTrack.IsRepeat);
            var block = BlockViews[0];
            Debug.Assert(block.Ordinal == gridOffset.BlockOrdinal);
            availableLength -= GetBlockLengthMain(block) - GetRelativeOffset(block, gridTrack, fraction);
            RealizeForward(availableLength);
        }

        private double GetRelativeOffset(BlockView block, GridTrack gridTrack, double fraction)
        {
            return gridTrack.GetRelativeSpan(block).StartOffset + GetMeasuredLength(block, gridTrack) * fraction;
        }

        private double RealizeForward(double availableLength)
        {
            if (BlockViews.Count == 0)
                return 0;

            Debug.Assert(BlockViews.Last != null);

            double result = 0;

            for (int blockOrdinal = BlockViews.Last.Ordinal + 1; blockOrdinal < MaxBlockCount && availableLength > 0; blockOrdinal++)
            {
                BlockViews.RealizeNext();
                var block = BlockViews.Last;
                block.Measure(Size.Empty);
                var measuredLength = GetBlockLengthMain(block);
                result += measuredLength;
                availableLength -= measuredLength;
            }
            return result;
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
            var valueMain = CoerceViewportMain();
            var valueCross = CoerceViewportCross();
            RefreshViewport(valueMain, valueCross);
        }

        private double CoerceViewportMain()
        {
            if (GridTracksMain.SizeToContent)
                return MaxOffsetMain;

            var result = GridTracksMain.AvailableLength;
            var frozenLength = FrozenHeadLengthMain + FrozenTailLengthMain;
            return Math.Max(frozenLength, result);
        }

        private double CoerceViewportCross()
        {
            if (GridTracksCross.SizeToContent)
                return MaxOffsetCross;

            var result = GridTracksCross.AvailableLength;
            return result;
        }

        private void RefreshScrollOffset()
        {
            var scrollOriginMain = GetOffset(ScrollOriginMain);
            var scrollStartMain = GetOffset(_scrollStartMain);
            Debug.Assert(scrollStartMain >= scrollOriginMain);
            var valueMain = scrollStartMain - scrollOriginMain;
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

        private bool IsFrozenHeadMain(ScalarItem scalarItem)
        {
            return GridTracksMain.GetGridSpan(scalarItem.GridRange).EndTrack.Ordinal < FrozenHeadMain;
        }

        private bool IsFrozenHeadCross(ScalarItem scalarItem)
        {
            return GridTracksCross.GetGridSpan(scalarItem.GridRange).EndTrack.Ordinal < FrozenHeadCross;
        }

        private bool IsFrozenTailMain(ScalarItem scalarItem)
        {
            return GridTracksMain.GetGridSpan(scalarItem.GridRange).StartTrack.Ordinal >= GridTracksMain.Count - FrozenTailMain;
        }

        private bool IsFrozenTailCross(ScalarItem scalarItem)
        {
            return GridTracksCross.GetGridSpan(scalarItem.GridRange).StartTrack.Ordinal >= GridTracksCross.Count - FrozenTailCross;
        }

        private bool IsFrozenCross(TemplateItem templateItem)
        {
            var gridSpan = GridTracksCross.GetGridSpan(templateItem.GridRange);
            return gridSpan.EndTrack.Ordinal < GridTracksCross.FrozenHead || gridSpan.StartTrack.Ordinal >= GridTracksCross.Count - GridTracksCross.FrozenTail;
        }

        protected sealed override Size GetScalarItemSize(ScalarItem scalarItem)
        {
            var valueMain = GetScalarItemLengthMain(scalarItem);
            var valueCross = GetScalarItemLengthCross(scalarItem);
            return ToSize(valueMain, valueCross);
        }

        private double GetScalarItemLengthMain(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startGridOffset = GetStartGridOffset(gridRange);
            var endGridOffset = GetEndGridOffset(gridRange);
            return startGridOffset == endGridOffset ? startGridOffset.Span.Length : endGridOffset.Span.EndOffset - startGridOffset.Span.StartOffset;
        }

        private double GetScalarItemLengthCross(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);
            if (BlockDimensions > 1 && !scalarItem.IsMultidimensional)
            {
                var rowSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                var scalarItemSpan = GridTracksCross.GetGridSpan(gridRange);
                if (rowSpan.Contains(scalarItemSpan))
                    valueCross += BlockDimensionLength * (BlockDimensions - 1);
            }
            return valueCross;
        }

        protected override Point GetScalarItemLocation(ScalarItem scalarItem, int blockDimension)
        {
            var valueMain = GetScalarItemStartMain(scalarItem);
            var valueCross = GetScalarItemStartCross(scalarItem, blockDimension);
            return ToPoint(valueMain, valueCross);
        }

        private double GetScalarItemStartMain(ScalarItem scalarItem)
        {
            var gridRange = scalarItem.GridRange;
            var startGridOffset = GetStartGridOffset(gridRange);
            var valueMain = startGridOffset.IsEof ? MaxOffsetMain : startGridOffset.Span.StartOffset;

            if (IsFrozenHeadMain(scalarItem))
                return valueMain;

            valueMain -= ScrollOffsetMain;

            if (IsFrozenTailMain(scalarItem))
            {
                double maxValueMain = ViewportMain - (MaxOffsetMain - startGridOffset.Span.StartOffset);
                if (valueMain > maxValueMain)
                    valueMain = maxValueMain;
            }

            return valueMain;
        }

        private double GetScalarItemStartCross(ScalarItem scalarItem, int blockDimension)
        {
            var startTrack = GridTracksCross.GetGridSpan(scalarItem.GridRange).StartTrack;
            var valueCross = startTrack.StartOffset;

            if (IsFrozenHeadCross(scalarItem))
                return valueCross;

            valueCross -= ScrollOffsetCross;
            if (blockDimension > 0)
                valueCross += blockDimension * BlockDimensionLength;

            if (IsFrozenTailCross(scalarItem))
            {
                double maxValueCross = ViewportCross - GetTailLengthCross(GridTracksCross.Count - startTrack.Ordinal);
                if (valueCross > maxValueCross)
                    valueCross = maxValueCross;
            }

            return valueCross;
        }

        internal override Thickness GetScalarItemClip(ScalarItem scalarItem, int blockDimension)
        {
            var clipMain = GetScalarItemClipMain(scalarItem);
            var clipCross = GetScalarItemClipCross(scalarItem, blockDimension);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetScalarItemClipMain(ScalarItem scalarItem)
        {
            if (IsFrozenHeadMain(scalarItem) || IsFrozenTailMain(scalarItem))
                return new Clip();

            var start = GetScalarItemStartMain(scalarItem);
            var end = start + GetScalarItemLengthMain(scalarItem);
            double? minStart = FrozenHeadMain == 0 ? new double?() : FrozenHeadLengthMain;
            double? maxEnd = FrozenTailMain == 0 ? new double?() : ViewportMain - FrozenTailLengthMain;
            return new Clip(start, end, minStart, maxEnd);
        }

        private Clip GetScalarItemClipCross(ScalarItem scalarItem, int blockDimension)
        {
            if (IsFrozenHeadCross(scalarItem) || IsFrozenTailCross(scalarItem))
                return new Clip();

            var start = GetScalarItemStartCross(scalarItem, blockDimension);
            var end = start + GetScalarItemLengthCross(scalarItem);
            double? minStart = FrozenHeadCross == 0 ? new double?() : FrozenHeadLengthCross;
            double? maxEnd = FrozenTailCross == 0 ? new double?() : ViewportCross - FrozenTailLengthCross;
            return new Clip(start, end, minStart, maxEnd);
        }

        protected override Size GetMeasuredSize(BlockView block, GridRange gridRange, bool clipScrollCross)
        {
            Debug.Assert(!gridRange.IsEmpty && Template.BlockRange.Contains(gridRange));

            var valueMain = GetMeasuredLengthMain(block, gridRange);
            var valueCross = GetMeasuredLengthCross(block, gridRange, clipScrollCross);
            return ToSize(valueMain, valueCross);
        }

        private double GetMeasuredLengthCross(BlockView block, GridRange gridRange, bool clipScrollCross)
        {
            var valueCross = GridTracksCross.GetMeasuredLength(gridRange);
            if (clipScrollCross)
                valueCross -= GetScrollCrossClip(gridRange);
            return valueCross;
        }

        private double GetMeasuredLengthMain(BlockView block, GridRange gridRange)
        {
            var gridSpan = GridTracksMain.GetGridSpan(gridRange);
            var startTrack = gridSpan.StartTrack;
            var endTrack = gridSpan.EndTrack;
            return startTrack == endTrack ? startTrack.GetRelativeSpan(block).Length
                : endTrack.GetRelativeSpan(block).EndOffset - startTrack.GetRelativeSpan(block).StartOffset;
        }

        private double GetScrollCrossClip(GridRange gridRange)
        {
            var span = GridTracksCross.GetGridSpan(gridRange);
            var startOffset = span.StartTrack.StartOffset;
            if (ScrollOriginCross <= startOffset)
                return 0;
            var endOffset = span.EndTrack.EndOffset;
            var scrollStart = ScrollStartCross;
            return scrollStart > startOffset && scrollStart < endOffset ? scrollStart - Math.Max(startOffset, ScrollOriginCross) : 0;
        }

        private GridOffset GetStartGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, 0);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        private GridOffset GetEndGridOffset(GridRange gridRange)
        {
            var gridTrack = GridTracksMain.GetGridSpan(gridRange).EndTrack;
            if (!gridTrack.IsRepeat)
                return new GridOffset(gridTrack);

            if (MaxBlockCount > 0)
                return new GridOffset(gridTrack, MaxBlockCount - 1);
            else
                return MaxFrozenTailMain > 0 ? new GridOffset(GridTracksMain.LastOf(MaxFrozenTailMain)) : GridOffset.Eof;
        }

        protected override Point GetBlockLocation(BlockView block)
        {
            var valueMain = GetBlockStartMain(block);
            var valueCross = GetBlockStartCross(block);
            return ToPoint(valueMain, valueCross);
        }

        private double GetBlockStartMain(BlockView block)
        {
            return GetStartOffset(block) - ScrollOffsetMain;
        }

        private double GetBlockStartCross(BlockView block)
        {
            return GridTracksCross.BlockStart.StartOffset - ScrollOffsetCross;
        }

        protected override Size GetBlockSize(BlockView block)
        {
            var valueMain = GetBlockLengthMain(block);
            var valueCross = GetBlockLengthCross(block);
            return ToSize(valueMain, valueCross);
        }

        private double GetBlockLengthMain(BlockView block)
        {
            return GetMeasuredLengthMain(block, Template.BlockRange);
        }

        private double GetBlockLengthCross(BlockView block)
        {
            var valueCross = GetMeasuredLengthCross(block, Template.BlockRange, true);
            if (BlockDimensions > 1)
                valueCross += BlockDimensionLength * (BlockDimensions - 1);
            return valueCross;
        }

        internal override Thickness GetBlockClip(BlockView block)
        {
            var clipMain = GetBlockClipMain(block);
            var clipCross = GetBlockClipCross(block);
            return ToThickness(clipMain, clipCross);
        }

        private Clip GetBlockClipMain(BlockView block)
        {
            var start = GetBlockStartMain(block);
            var end = start + GetBlockLengthMain(block);
            double? minStart = FrozenHeadMain == 0 ? new double?() : FrozenHeadLengthMain;
            double? maxEnd = FrozenTailMain == 0 ? new double?() : ViewportMain - FrozenTailLengthMain;
            return new Clip(start, end, minStart, maxEnd);
        }

        private Clip GetBlockClipCross(BlockView block)
        {
            return new Clip();
        }

        private double GetStartOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockStart, block).Span.StartOffset;
        }

        private double GetEndOffset(BlockView block)
        {
            return new GridOffset(GridTracksMain.BlockEnd, block).Span.EndOffset;
        }

        private Point GetRelativeLocation(BlockView block, GridRange gridRange)
        {
            Debug.Assert(Template.BlockRange.Contains(gridRange));

            var startTrackMain = GridTracksMain.GetGridSpan(gridRange).StartTrack;
            var valueMain = startTrackMain.GetRelativeSpan(block).StartOffset;
            var valueCross = GridTracksCross.GetGridSpan(gridRange).StartTrack.StartOffset - GridTracksCross.BlockStart.StartOffset;
            if (BlockDimensions > 1)
            {
                var rowGridSpan = GridTracksCross.GetGridSpan(Template.RowRange);
                if (valueCross >= rowGridSpan.EndTrack.EndOffset)
                    valueCross += rowGridSpan.MeasuredLength * (BlockDimensions - 1);
            }
            return ToPoint(valueMain, valueCross);
        }

        protected override Point GetBlockItemLocation(BlockView block, BlockItem blockItem)
        {
            return GetRelativeLocation(block, blockItem.GridRange);
        }

        protected override Point GetRowLocation(BlockView block, int blockDimension)
        {
            var result = GetRelativeLocation(block, Template.RowRange);
            if (blockDimension > 0)
                result += blockDimension * BlockDimensionVector;
            return result;
        }

        protected override Point GetRowItemLocation(BlockView block, RowItem rowItem)
        {
            var rowLocation = GetRelativeLocation(block, Template.RowRange);
            var result = GetRelativeLocation(block, rowItem.GridRange);
            result.Offset(-rowLocation.X, -rowLocation.Y);
            return result;
        }

        protected sealed override Size MeasuredSize
        {
            get { return new Size(ViewportX, ViewportY); }
        }
    }
}
