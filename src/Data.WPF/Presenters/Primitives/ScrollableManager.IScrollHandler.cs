﻿using DevZest.Data.Views.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Presenters.Primitives
{
    partial class ScrollableManager
    {
        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        public abstract double ViewportWidth { get; }

        public abstract double ViewportHeight { get; }

        public abstract double ScrollableWidth { get; }

        public abstract double ScrollableHeight { get; }

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

        public abstract double ExtentHeight { get; }

        public abstract double ExtentWidth { get; }

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

        public abstract double HorizontalOffset { get; }

        public abstract double VerticalOffset { get; }

        private LogicalExtent _scrollToMain;
        private GridPlacement _scrollToMainPlacement;
        private double? _scrollOffsetMain;
        protected double ScrollOffsetMain
        {
            get { return _scrollOffsetMain.HasValue ? _scrollOffsetMain.GetValueOrDefault() : Translate(_scrollToMain) - Translate(MinScrollToMain); }
        }

        private double _scrollDeltaMain;
        private void SetScrollDeltaMain(double value, bool invalidateMeasure)
        {
            if (_scrollDeltaMain.IsClose(value))
                return;
            _scrollDeltaMain = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private double _scrollDeltaCross;
        private void SetScrollDeltaCross(double value, bool invalidateMeasure)
        {
            if (_scrollDeltaCross.IsClose(value))
                return;
            _scrollDeltaCross = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private static double CoerceScrollOffset(double value, double maxValue)
        {
            if (value < 0)
                value = 0;
            if (value > maxValue)
                value = maxValue;
            return value;
        }

        protected double ScrollOffsetCross { get; private set; }

        private void RefreshScrollOffset(double valueMain, double valueCross)
        {
            bool invalidateScrollInfo = !(_scrollOffsetMain.HasValue && _scrollOffsetMain.Value.IsClose(valueMain)) || !ScrollOffsetCross.IsClose(valueCross);
            _scrollOffsetMain = valueMain;
            ScrollOffsetCross = valueCross;
            SetScrollDeltaMain(0, false);
            SetScrollDeltaCross(0, false);
            if (invalidateScrollInfo)
                InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            EnsureVisible(visual);
            return rectangle;
        }

        public abstract int MaxGridExtentX { get; }

        public abstract int MaxGridExtentY { get; }

        protected int MaxGridExtentMain
        {
            get { return HeadTracksCountMain + TotalContainerGridTracksMain + TailTracksCountMain; }
        }

        protected int MaxGridExtentCross
        {
            get
            {
                return HeadTracksCountCross + TailTracksCountCross
                  + FlowRepeatCount * RowTracksCountCross + (ContainerTracksCountCross - RowTracksCountCross);
            }
        }

        public abstract int FrozenHeadGridExtentX { get; }

        public abstract int FrozenTailGridExtentX { get; }

        public abstract int FrozenHeadGridExtentY { get; }

        public abstract int FrozenTailGridExtentY { get; }

        protected int FrozenHeadGridExtentMain
        {
            get { return FrozenHeadTracksCountMain; }
        }

        protected int FrozenHeadGridExtentCross
        {
            get { return FrozenHeadTracksCountCross; }
        }

        protected int FrozenTailGridExtentMain
        {
            get { return MaxGridExtentMain - FrozenTailTracksCountMain; }
        }

        protected int FrozenTailGridExtentCross
        {
            get { return MaxGridExtentCross - FrozenTailTracksCountCross; }
        }

        public double GetExtentX(int gridExtentX)
        {
            VerifyGridExtent(gridExtentX, nameof(gridExtentX), MaxGridExtentX);
            return GetExtentXCore(gridExtentX);
        }

        protected abstract double GetExtentXCore(int gridExtentX);

        public double GetExtentY(int gridExtentY)
        {
            VerifyGridExtent(gridExtentY, nameof(gridExtentY), MaxGridExtentY);
            return GetExtentYCore(gridExtentY);
        }

        protected abstract double GetExtentYCore(int gridExtentY);

        protected double GetExtentMain(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);
            return gridExtent >= MaxGridExtentMain
                ? GetLogicalMainTrack(MaxGridExtentMain - 1).EndExtent
                : GetLogicalMainTrack(gridExtent).StartExtent;
        }

        protected double GetExtentCross(int gridExtent)
        {
            Debug.Assert(gridExtent >= 0);
            return gridExtent >= MaxGridExtentCross
                ? GetLogicalCrossTrack(MaxGridExtentCross - 1).EndExtent
                : GetLogicalCrossTrack(gridExtent).StartExtent;
        }

        public double GetPositionX(int gridExtentX, GridPlacement placement)
        {
            VerifyGridExtent(gridExtentX, nameof(gridExtentX), placement, nameof(placement), MaxGridExtentX);
            return GetPositionXCore(gridExtentX, placement);
        }

        protected abstract double GetPositionXCore(int gridExtentX, GridPlacement placement);

        public double GetPositionY(int gridExtentY, GridPlacement placement)
        {
            VerifyGridExtent(gridExtentY, nameof(gridExtentY), placement, nameof(placement), MaxGridExtentY);
            return GetPositionYCore(gridExtentY, placement);
        }

        protected abstract double GetPositionYCore(int gridExtentY, GridPlacement placement);

        protected double GetPositionMain(int gridExtent, GridPlacement placement)
        {
            return placement == GridPlacement.Tail
                ? GetLogicalMainTrack(gridExtent - 1).EndPosition
                : GetLogicalMainTrack(gridExtent).StartPosition;
        }

        protected double GetPositionCross(int gridExtent, GridPlacement placement)
        {
            return placement == GridPlacement.Tail
                ? GetLogicalCrossTrack(gridExtent - 1).EndPosition
                : GetLogicalCrossTrack(gridExtent).StartPosition;
        }

        private void VerifyGridExtent(int gridExtent, string gridExtentParamName, int maxGridExtent)
        {
            if (gridExtent < 0 || gridExtent > maxGridExtent)
                throw new ArgumentOutOfRangeException(gridExtentParamName);
        }

        private void VerifyGridExtent(int gridExtent, string gridExtentParamName,
            GridPlacement placement, string placementParamName, int maxGridExtent)
        {
            VerifyGridExtent(gridExtent, gridExtentParamName, maxGridExtent);

            if (gridExtent == 0 && placement == GridPlacement.Tail)
                throw new ArgumentException(DiagnosticMessages.GridPlacement_InvalidTailValue, placementParamName);

            if (gridExtent == maxGridExtent && placement == GridPlacement.Head)
                throw new ArgumentException(DiagnosticMessages.GridPlacement_InvalidHeadValue, placementParamName);
        }

        public abstract void ScrollToX(int gridExtent, double fraction, GridPlacement placement);

        public abstract void ScrollToY(int gridExtent, double fraction, GridPlacement placement);

        protected void ScrollToMain(int gridExtent, double fraction, GridPlacement placement, bool virtualizeAll)
        {
            VerifyLogicalExtent(gridExtent, fraction, MaxGridExtentMain);
            if (_scrollToMain.GridExtent == gridExtent && _scrollToMain.Fraction.IsClose(fraction)
                && _scrollToMainPlacement == placement
                && _scrollDeltaMain == 0)
                return;
            ScrollToMain(new LogicalExtent(gridExtent, fraction), placement);
            if (virtualizeAll)
                ContainerViewList.VirtualizeAll();
        }

        private void VerifyLogicalExtent(int gridExtent, double fraction, int maxGridExtent)
        {
            if (fraction < 0 || fraction > 1)
                throw new ArgumentOutOfRangeException(nameof(fraction));
            if (gridExtent < 0 || gridExtent + fraction > maxGridExtent)
                throw new ArgumentOutOfRangeException(nameof(gridExtent));
        }

        protected void ScrollToCross(int gridExtent, double fraction, GridPlacement placement)
        {
            VerifyLogicalExtent(gridExtent, fraction, MaxGridExtentCross);
            var logicalTrack = GetLogicalCrossTrack(gridExtent);
            Debug.Assert(!logicalTrack.IsEof);
            if (placement == GridPlacement.Head)
            {
                var extent = logicalTrack.StartExtent + logicalTrack.GridTrack.MeasuredLength * fraction;
                InternalScrollBy(0, extent - FrozenHeadLengthCross - ScrollOffsetCross);
            }
            else
            {
                var position = logicalTrack.EndPosition - logicalTrack.GridTrack.MeasuredLength * (1 - fraction);
                InternalScrollBy(0, position - (ViewportCross - FrozenTailLengthCross));
            }
        }

        private void ScrollToMain(LogicalExtent scrollTo, GridPlacement placement)
        {
            _scrollToMain = scrollTo;
            _scrollToMainPlacement = placement;
            SetScrollDeltaMain(0, false);
            InvalidateMeasure();
        }

        public void ScrollTo(double horizontalOffset, double verticalOffset)
        {
            var offsetX = double.IsNaN(horizontalOffset) ? 0 : horizontalOffset - HorizontalOffset;
            var offsetY = double.IsNaN(verticalOffset) ? 0 : verticalOffset - VerticalOffset;
            ScrollBy(offsetX, offsetY);
        }

        public abstract void ScrollBy(double x, double y);

        protected void InternalScrollBy(double valueMain, double valueCross)
        {
            var scrollTo = TryScrollToMain(valueMain);
            if (scrollTo.HasValue)
                ScrollToMain(scrollTo.GetValueOrDefault(), _scrollToMainPlacement);
            else
                SetScrollDeltaMain(valueMain, true);
            SetScrollDeltaCross(valueCross, true);
        }

        private LogicalExtent? TryScrollToMain(double scrollByMain)
        {
            var result = Translate(Translate(_scrollToMain) + _scrollDeltaMain + scrollByMain);
            var resultMainTrack = GetLogicalMainTrack(result.GridExtent);
            var currentMainTrack = GetLogicalMainTrack(_scrollToMain.GridExtent);

            if (IsBroken(resultMainTrack, currentMainTrack))
                return null;
            else
                return result;
        }

        private bool IsBroken(LogicalMainTrack result, LogicalMainTrack current)
        {
            if (result.IsEof || current.IsEof)
                return true;

            if (result.IsContainer)
            {
                if (IsVirtualized(result.ContainerOrdinal))
                    return true;

                return IsBrokenToContainer(current);
            }
            else if (result.IsHead)
                return IsBrokenToHead(current);
            else
            {
                Debug.Assert(result.IsTail);
                return IsBrokenToTail(current);
            }
        }

        private bool IsBrokenToContainer(LogicalMainTrack track)
        {
            Debug.Assert(!track.IsEof);
            if (track.IsContainer)
                return IsVirtualized(track.ContainerOrdinal);
            else if (track.IsHead)
                return IsFirstRowVirtualized;
            else
            {
                Debug.Assert(track.IsTail);
                return !IsLastRowVirtualized;
            }
        }

        private bool IsBrokenToHead(LogicalMainTrack track)
        {
            Debug.Assert(!track.IsEof);
            if (track.IsHead)
                return false;
            else if (track.IsContainer)
                return IsFirstRowVirtualized;
            else
            {
                Debug.Assert(track.IsTail);
                return ContainerViewList.Count > 0;
            }
        }

        private bool IsBrokenToTail(LogicalMainTrack track)
        {
            Debug.Assert(!track.IsEof);

            if (track.IsTail)
                return false;
            else if (track.IsContainer)
                return IsLastRowVirtualized;
            else
            {
                Debug.Assert(track.IsHead);
                return ContainerViewList.Count > 0;
            }
        }

        private bool IsVirtualized(int containerOrdinal)
        {
            return ContainerViewList.GetContainerView(containerOrdinal) == null;
        }

        private bool IsFirstRowVirtualized
        {
            get
            {
                Debug.Assert(ContainerViewList.First != null);
                return ContainerViewList.First.ContainerOrdinal > 0;
            }
        }

        private bool IsLastRowVirtualized
        {
            get
            {
                Debug.Assert(ContainerViewList.Last != null);
                return ContainerViewList.Last.ContainerOrdinal < MaxContainerCount - 1;
            }
        }

        public void EnsureCurrentRowVisible()
        {
            if (CurrentRow == null || Panel == null)
                return;

            if (!Panel.IsMeasureValid)
                UpdateLayout();
            EnsureVisibleMain(CurrentRow.View);
            EnsureVisibleCross(CurrentRow.View);
        }

        private void UpdateLayout()
        {
            Panel.UpdateLayout();
        }

        protected sealed override void PrepareMeasureContainers()
        {
            _variantLengthHandler?.ResetMeasuredLengths();
            _scrollOffsetMain = null;
            if (!UpdateScrollToMain())
                CoerceScrollToMain();
            if (_scrollToMainPlacement == GridPlacement.Head)
            {
                InitContainerViewsForward();
                FillForward();
            }
            else
            {
                InitContainerViewsBackward();
                FillBackward();
            }
        }

        private bool UpdateScrollToMain()
        {
            if (_scrollDeltaMain == 0 || Math.Abs(_scrollDeltaMain) <= ViewportMain)
                return false;

            AdjustScrollToMain(false);
            ContainerViewList.VirtualizeAll();
            return true;
        }

        private void AdjustScrollToMain(bool assertRealized)
        {
            AdjustScrollToMain(_scrollDeltaMain, assertRealized);
        }

        private void AdjustScrollToMain(double delta, bool assertRealized)
        {
            _scrollToMain = Translate(ScrollToMainExtent + delta);
            SetScrollDeltaMain(0, false);
            CoerceScrollToMain();
            if (assertRealized)
                AssertAdjustedScrollToMain();
        }

        [Conditional("DEBUG")]
        private void AssertAdjustedScrollToMain()
        {
            var scrollTo = GetLogicalMainTrack(_scrollToMain.GridExtent);
            if (scrollTo.IsContainer)
                Debug.Assert(ContainerViewList.GetContainerView(scrollTo.ContainerOrdinal) != null, "_scrollToMain must be realized ContainerView.");
        }

        private void CoerceScrollToMain()
        {
            var min = MinScrollToMain;
            if (_scrollToMain.Value < min.Value)
                _scrollToMain = min;
            var max = MaxScrollToMain;
            if (_scrollToMain.Value > max.Value)
                _scrollToMain = max;
        }

        private void InitContainerViewsForward()
        {
            var initialOrdinal = GetInitialOrdinal();
            if (initialOrdinal < 0)
                return;

            if (ContainerViewList.Count == 0 || initialOrdinal > ContainerViewList.Last.ContainerOrdinal)
            {
                ContainerViewList.VirtualizeAll();
                ContainerViewList.RealizeFirst(initialOrdinal);
            }
            else if (initialOrdinal < ContainerViewList.First.ContainerOrdinal)
            {
                for (int i = ContainerViewList.First.ContainerOrdinal - 1; i >= initialOrdinal; i--)
                {
                    ContainerViewList.RealizePrev();
                    ContainerViewList[0].Measure(Size.Empty);
                }
            }
            else
            {
                for (int i = ContainerViewList.First.ContainerOrdinal; i < initialOrdinal; i++)
                    ContainerViewList.VirtualizeFirst();
            }

            for (int i = 0; i < ContainerViewList.Count; i++)
                ContainerViewList[i].Measure(Size.Empty);
        }

        private void InitContainerViewsBackward()
        {
            var initialOrdinal = GetInitialOrdinal();
            if (initialOrdinal < 0)
                return;

            if (ContainerViewList.Count == 0 || initialOrdinal < ContainerViewList.First.ContainerOrdinal)
            {
                ContainerViewList.VirtualizeAll();
                ContainerViewList.RealizeFirst(initialOrdinal);
            }
            else if (initialOrdinal > ContainerViewList.Last.ContainerOrdinal)
            {
                for (int i = ContainerViewList.Last.ContainerOrdinal + 1; i <= initialOrdinal; i++)
                {
                    ContainerViewList.RealizeNext();
                    ContainerViewList.Last.Measure(Size.Empty);
                }
            }
            else
            {
                for (int i = ContainerViewList.Last.ContainerOrdinal; i > initialOrdinal; i++)
                    ContainerViewList.VirtualizeLast();
            }

            for (int i = 0; i < ContainerViewList.Count; i++)
                ContainerViewList[i].Measure(Size.Empty);
        }

        private int GetInitialOrdinal()
        {
            if (MaxContainerCount == 0)
                return -1;

            var logicalMainTrack = GetLogicalMainTrack(_scrollToMain.GridExtent);
            if (logicalMainTrack.IsEof)
                return MaxContainerCount - 1;

            var gridTrack = logicalMainTrack.GridTrack;
            if (gridTrack.IsHead)
                return 0;
            else if (gridTrack.IsContainer)
                return logicalMainTrack.ContainerOrdinal;
            else
                return MaxContainerCount - 1;
        }

        private double ScrollableLength
        {
            get { return GridTracksMain.AvailableLength - FrozenHeadLengthMain - FrozenTailLengthMain; }
        }

        private void FillForward()
        {
            if (ContainerViewList.Count == 0)
                return;

            var existingFilledLength = GetExistingFillForwardLength();
            var lengthToFillForward = Math.Max(0, ScrollableLength + _scrollDeltaMain - existingFilledLength);
            var lengthFilledForward = RealizeForward(lengthToFillForward);

            var lengthToFillBackward = Math.Max(-_scrollDeltaMain, 0) + Math.Max(0, lengthToFillForward - lengthFilledForward);
            var lengthFilledBackward = RealizeBackward(lengthToFillBackward);

            var extraFillForward = Math.Max(0, ScrollableLength - existingFilledLength - lengthFilledForward - lengthFilledBackward);
            RealizeForward(extraFillForward);
        }

        private void FillBackward()
        {
            if (ContainerViewList.Count == 0)
                return;

            var existingFilledLength = GetExistingFillBackwardLength();
            var lengthToFillBackward = Math.Max(0, ScrollableLength - _scrollDeltaMain - existingFilledLength);
            var lengthFilledBackward = RealizeBackward(lengthToFillBackward);

            var lengthToFillForward = Math.Max(-_scrollDeltaMain, 0) + Math.Max(0, lengthToFillBackward - lengthFilledBackward);
            var lengthFilledForward = RealizeForward(lengthToFillForward);

            var extraFillForward = Math.Max(0, ScrollableLength - existingFilledLength - lengthFilledBackward - lengthFilledForward);
            RealizeForward(extraFillForward);
        }

        private double GetExistingFillForwardLength()
        {
            var start = GetLogicalMainTrack(_scrollToMain.GridExtent);
            var fraction = _scrollToMain.Fraction;

            int startIndex = 0;
            double result = 0;
            if (start.IsContainer)
            {
                result += GetRelativePositionMain(ContainerViewList.GetContainerView(start.ContainerOrdinal), start.GridTrack, 1 - fraction);
                startIndex++;
            }
            for (int i = startIndex; i < ContainerViewList.Count; i++)
                result += GetLengthMain(ContainerViewList[i]);
            return result;
        }

        private double GetExistingFillBackwardLength()
        {
            var end = GetLogicalMainTrack(_scrollToMain.GridExtent);
            var fraction = _scrollToMain.Fraction;

            int endIndex = ContainerViewList.Count - 1;
            double result = 0;
            if (end.IsContainer)
            {
                result += GetRelativePositionMain(ContainerViewList.GetContainerView(end.ContainerOrdinal), end.GridTrack, fraction);
                endIndex--;
            }
            for (int i = endIndex; i >= 0; i--)
                result += GetLengthMain(ContainerViewList[i]);
            return result;
        }

        private bool CanRealizeForward
        {
            get { return ContainerViewList.Last != null && ContainerViewList.Last.ContainerOrdinal < MaxContainerCount - 1; }
        }

        private bool CanRealizeBackward
        {
            get { return ContainerViewList.First != null && ContainerViewList.First.ContainerOrdinal > 0; }
        }

        private double GetRelativePositionMain(ContainerView containerView, GridTrack gridTrack, double fraction)
        {
            Debug.Assert(gridTrack.IsContainer);
            Debug.Assert(fraction >= 0 && fraction <= 1);

            var result = GetRelativePositionMain(containerView, gridTrack);
            if (fraction > 0)
                result += new LogicalMainTrack(gridTrack, containerView).Length * fraction;
            return result;
        }


        private double RealizeForward(double length)
        {
            Debug.Assert(length >= 0);
            if (length == 0 || !CanRealizeForward)
                return 0;

            double result = 0;
            for (int ordinal = ContainerViewList.Last.ContainerOrdinal + 1; ordinal < MaxContainerCount; ordinal++)
            {
                ContainerViewList.RealizeNext();
                var containerView = ContainerViewList.Last;
                containerView.Measure(Size.Empty);
                var measuredLength = GetLengthMain(containerView);
                result += measuredLength;
                length -= measuredLength;
                if (length <= 0)
                    break;
            }
            return result;
        }

        private double RealizeBackward(double length)
        {
            Debug.Assert(length >= 0);
            if (length == 0 || !CanRealizeBackward)
                return 0;

            double result = 0;
            for (int ordinal = ContainerViewList.First.ContainerOrdinal - 1; ordinal >= 0; ordinal--)
            {
                ContainerViewList.RealizePrev();
                var containerView = ContainerViewList.First;
                containerView.Measure(Size.Empty);
                var measuredLength = GetLengthMain(containerView);
                result += measuredLength;
                length -= measuredLength;
                if (length <= 0)
                    break;
            }
            return result;
        }

        protected sealed override void PrepareMeasure()
        {
            base.PrepareMeasure();

            // The following operations must be done after ScalarBindings.PostAutoSizeBindings measured.
            AdjustScrollToMain();
            AdjustToFillGap();
            RefreshScrollInfo();
            if (RemoveInvisibleContainerViews() && _variantLengthHandler != null)
                RefreshScrollInfo();
        }

        private void AdjustScrollToMain()
        {
            if (_scrollDeltaMain != 0)
                AdjustScrollToMain(true);

            if (_scrollToMainPlacement == GridPlacement.Tail)
            {
                _scrollToMainPlacement = GridPlacement.Head;
                var delta = GridTracksMain.AvailableLength - FrozenTailLengthMain - FrozenHeadLengthMain;
                AdjustScrollToMain(-delta, true);
            }
        }

        private void RefreshScrollInfo()
        {
            RefreshViewport();
            RefreshExtent();  // Exec order matters: RefreshExtent relies on RefreshViewport
            RefreshScrollOffset();  // Exec order matters: RefreshScrollOffset relies on RefreshViewport and RefreshExtent
        }

        private void AdjustToFillGap()
        {
            var headGapToAdjust = HeadGapToAdjust;
            if (headGapToAdjust > 0 && !headGapToAdjust.IsClose(0))
                AdjustScrollToMain(-headGapToAdjust, true);

            var tailGapToAdjust = TailGapToAdjust;
            if (tailGapToAdjust > 0 && !tailGapToAdjust.IsClose(0))
                AdjustScrollToMain(-tailGapToAdjust, true);
        }

        private double HeadGapToAdjust
        {
            get
            {
                var first = ContainerViewList.First;
                if (first == null)
                    return 0;
                var firstStartPosition = GetStartPositionMain(first);
                return Math.Max(0, firstStartPosition - ScrollableStartPositionMain);
            }
        }

        private double ScrollableStartPositionMain
        {
            get
            {
                var result = FrozenHeadLengthMain;
                if (HeadTracksCountMain - FrozenHeadTracksCountMain > 0)
                    result = Math.Max(result, GetPositionMain(HeadTracksCountMain, GridPlacement.Tail));
                return result;
            }
        }

        private double TailGapToAdjust
        {
            get
            {
                var tailGap = TailGap;
                if (tailGap == 0)
                    return 0;

                var result = Math.Max(0, Translate(MinScrollToMain) - GetStartPositionMain(ContainerViewList.First));
                return Math.Min(result, tailGap);
            }
        }

        private double TailGap
        {
            get
            {
                var availableLength = GridTracksMain.AvailableLength;
                if (double.IsPositiveInfinity(availableLength))
                    return availableLength;

                var last = ContainerViewList.Last;
                if (last == null)
                    return 0;
                var lastEndPosition = GetEndPositionMain(last);
                var scrollableEnd = availableLength - FrozenTailLengthMain;
                if (TailTracksCountMain - FrozenTailTracksCountMain > 0)
                    scrollableEnd -= GetTailLengthMain(TailTracksCountMain) - FrozenTailLengthMain;
                return scrollableEnd > lastEndPosition ? scrollableEnd - lastEndPosition : 0;
            }
        }

        private bool RemoveInvisibleContainerViews()
        {
            var countHead = HeadInvisibleContainerViewsCount;
            var countTail = TailInvisibleContainerViewsCount;

            for (int i = 0; i < countHead; i++)
                ContainerViewList.VirtualizeFirst();

            for (int i = 0; i < countTail; i++)
                ContainerViewList.VirtualizeLast();

            return countHead > 0 || countTail > 0;
        }

        private int HeadInvisibleContainerViewsCount
        {
            get
            {
                var result = 0;
                var startPosition = FrozenHeadLengthMain;
                for (int i = 0; i < ContainerViewList.Count - 1; i++)
                {
                    if (GetEndPositionMain(ContainerViewList[i]) < startPosition)
                        result++;
                    else
                        break;
                }
                return result;
            }
        }

        private int TailInvisibleContainerViewsCount
        {
            get
            {
                var result = 0;
                var endPosition = GridTracksMain.AvailableLength - FrozenTailLengthMain;
                for (int i = ContainerViewList.Count - 1; i > 0; i--)
                {
                    if (GetStartPositionMain(ContainerViewList[i]) >= endPosition)
                        result++;
                    else
                        break;
                }
                return result;
            }
        }

        private void RefreshExtent()
        {
            var valueMain = MaxExtentMain;
            if (valueMain < ViewportMain)
                valueMain = ViewportMain;
            var valueCross = MaxExtentCross;
            if (valueCross < ViewportCross)
                valueCross = ViewportCross;
            RefreshExtent(valueMain, valueCross);
        }

        private void RefreshViewport()
        {
            var valueMain = CoerceViewport(GridTracksMain, MaxExtentMain, FrozenHeadLengthMain, FrozenTailLengthMain);
            var valueCross = CoerceViewport(GridTracksCross, MaxExtentCross, FrozenHeadLengthCross, FrozenTailLengthCross);
            RefreshViewport(valueMain, valueCross);
        }

        private static double CoerceViewport(IGridTrackCollection gridTracks, double maxExtent, double frozenHeadLength, double frozenTailLength)
        {
            if (gridTracks.SizeToContent)
                return maxExtent;

            var result = gridTracks.AvailableLength;
            var frozenLength = frozenHeadLength + frozenTailLength;
            return Math.Max(frozenLength, result);
        }

        private void RefreshScrollOffset()
        {
            var minScrollToMain = Translate(MinScrollToMain);
            var scrollToMainExtent = ScrollToMainExtent;
            Debug.Assert(scrollToMainExtent >= minScrollToMain);
            var valueMain = scrollToMainExtent - minScrollToMain;
            var valueCross = CoerceScrollOffset(ScrollOffsetCross + _scrollDeltaCross, ExtentCross - ViewportCross);
            RefreshScrollOffset(valueMain, valueCross);
        }

        public abstract void ScrollPageUp();

        public abstract void ScrollPageDown();

        public RowPresenter ScrollToPageUp()
        {
            return ScrollByPage(GridPlacement.Head);
        }

        public RowPresenter ScrollToPageDown()
        {
            return ScrollByPage(GridPlacement.Tail);
        }

        private RowPresenter ScrollByPage(GridPlacement placement)
        {
            UpdateLayout();
            if (ContainerViewList.Count == 0)
                return null;

            if (CurrentContainerViewPlacement == CurrentContainerViewPlacement.BeforeList ||
                CurrentContainerViewPlacement == CurrentContainerViewPlacement.AfterList)
            {
                EnsureVisibleMain(CurrentRow.View);
                UpdateLayout();
            }

            Debug.Assert(CurrentContainerViewPlacement == CurrentContainerViewPlacement.WithinList);

            int currentContainerOrdinal = CurrentContainerView.ContainerOrdinal;
            var containerOrdinal = GetContainerOrdinalByPage(placement, false);
            var scrolled = placement == GridPlacement.Head ? ScrollByPageUp(containerOrdinal) : ScrollByPageDown(containerOrdinal);
            if (scrolled)
            {
                UpdateLayout();
                containerOrdinal = GetContainerOrdinalByPage(placement, true);
            }

            return GetRowPresenter(containerOrdinal);
        }

        private bool ScrollByPageUp(int containerOrdinal)
        {
            if (containerOrdinal < CurrentContainerView.ContainerOrdinal)
                return false;
            ScrollPageUp();
            return true;
        }

        private bool ScrollByPageDown(int containerOrdinal)
        {
            if (containerOrdinal > CurrentContainerView.ContainerOrdinal)
                return false;
            ScrollPageDown();
            return true;
        }

        private int GetContainerOrdinalByPage(GridPlacement placement, bool enforceCurrent)
        {
            return placement == GridPlacement.Head ? GetPageHeadContainerOrdinal(enforceCurrent) : GetPageTailContainerOrdinal(enforceCurrent);
        }

        private RowPresenter GetRowPresenter(int containerOrdinal)
        {
            Debug.Assert(CurrentRow != null);
            var delta = containerOrdinal - CurrentContainerView.ContainerOrdinal;
            var index = Math.Min(Rows.Count - 1, CurrentRow.Index + delta * FlowRepeatCount);
            return Rows[index];
        }

        private int GetPageHeadContainerOrdinal(bool enforceCurrent)
        {
            if (ContainerViewList.Count == 0)
                return -1;

            var containerOrdinal = ContainerViewList.First.ContainerOrdinal;
            var startTrack = GridTracksMain.GetGridSpan(Template.ContainerRange).StartTrack;
            var startGridExtent = new LogicalMainTrack(startTrack, ContainerViewList.First).StartGridExtent;
            var isClipped = IsHeadClippedMain(startGridExtent);
            if (NextContainerAsPageHeader(containerOrdinal, isClipped, enforceCurrent))
            {
                containerOrdinal++;
                startGridExtent += ContainerTracksCountMain;
                isClipped = false;
            }

            if (enforceCurrent && isClipped)
                ScrollToMain(startGridExtent, 0, GridPlacement.Head, false);
            return containerOrdinal;
        }

        private bool NextContainerAsPageHeader(int startContainerOrdinal, bool isClipped, bool enforceCurrent)
        {
            if (!isClipped)
                return false;
            if (enforceCurrent && CurrentContainerView.ContainerOrdinal == startContainerOrdinal + 1)
                return false;
            return startContainerOrdinal < ContainerViewList.Last.ContainerOrdinal;
        }

        private int GetPageTailContainerOrdinal(bool enforceCurrent)
        {
            if (ContainerViewList.Count == 0)
                return -1;

            var containerOrdinal = ContainerViewList.Last.ContainerOrdinal;
            var endTrack = GridTracksMain.GetGridSpan(Template.ContainerRange).EndTrack;
            var endGridExtent = new LogicalMainTrack(endTrack, ContainerViewList.Last).EndGridExtent;
            var isClipped = IsTailClippedMain(endGridExtent);
            if (PreviousContainerAsPageTail(containerOrdinal, isClipped, enforceCurrent))
            {
                containerOrdinal--;
                endGridExtent -= ContainerTracksCountMain;
                isClipped = false;
            }

            if (enforceCurrent && isClipped)
                ScrollToMain(endGridExtent - 1, 1, GridPlacement.Tail, false);
            return containerOrdinal;
        }

        private bool PreviousContainerAsPageTail(int endContainerOrdinal, bool isClipped, bool enforceCurrent)
        {
            if (!isClipped)
                return false;
            if (enforceCurrent && CurrentContainerView.ContainerOrdinal == endContainerOrdinal - 1)
                return false;
            return endContainerOrdinal > ContainerViewList.First.ContainerOrdinal;
        }
    }
}
