﻿using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using DevZest.Data.Views;
using System;

namespace DevZest.Data.Presenters.Primitives
{
    partial class ScrollableManager
    {
        private sealed class VariantLengthHandler
        {
            public VariantLengthHandler(ScrollableManager scrollableManager)
            {
                Debug.Assert(scrollableManager != null);
                _scrollableManager = scrollableManager;
            }

            private readonly ScrollableManager _scrollableManager;

            private ContainerViewList ContainerViewList
            {
                get { return _scrollableManager.ContainerViewList; }
            }

            private GridSpan GridSpan
            {
                get { return _scrollableManager.ContainerGridSpan; }
            }

            private bool _isContainerLengthsValid = true;
            internal void InvalidateContainerLengths()
            {
                _isContainerLengthsValid = false;
            }

            private void RefreshContainerLengths()
            {
                if (_isContainerLengthsValid)
                    return;

                _isContainerLengthsValid = true; // Avoid re-entrance
                for (int i = 1; i < ContainerViewList.Count; i++)
                    ContainerViewList[i].StartOffset = GetEndOffset(ContainerViewList[i - 1]);

                var gridSpan = GridSpan;
                Debug.Assert(!gridSpan.IsEmpty);

                for (int i = 0; i < gridSpan.Count; i++)
                {
                    var gridTrack = gridSpan[i];
                    double totalLength = 0;
                    for (int j = 0; j < ContainerViewList.Count; j++)
                        totalLength += GetMeasuredLength(ContainerViewList[j], gridTrack);
                    gridTrack.VariantByContainerAvgLength = ContainerViewList.Count == 0 ? 1 : totalLength / ContainerViewList.Count;
                }

                for (int i = 1; i < gridSpan.Count; i++)
                    gridSpan[i].VariantByContainerStartOffset = GetEndOffset(gridSpan[i - 1]);
            }

            private double GetStartOffset(ContainerView containerView)
            {
                RefreshContainerLengths();
                return containerView.StartOffset;
            }

            private double GetEndOffset(ContainerView containerView)
            {
                return GetStartOffset(containerView) + GetMeasuredLength(containerView);
            }

            private double AvgLength
            {
                get
                {
                    var count = ContainerViewList.Count;
                    return count == 0 ? 1 : TotalLength / count;
                }
            }

            private double TotalLength
            {
                get
                {
                    Debug.Assert(ContainerViewList.Count > 0);
                    return GetEndOffset(ContainerViewList.Last) - GetStartOffset(ContainerViewList.First);
                }
            }

            private double[] GetCumulativeMeasuredLengths(ContainerView containerView)
            {
                return containerView.CumulativeMeasuredLengths ?? (containerView.CumulativeMeasuredLengths = InitCumulativeMeasuredLengths(containerView));
            }

            private double[] InitCumulativeMeasuredLengths(ContainerView containerView)
            {
                Debug.Assert(GridSpan.Count > 0);
                var result = new double[GridSpan.Count];
                ResetMeasuredLengths(containerView);
                return result;
            }

            public void ResetMeasuredLengths()
            {
                if (_scrollableManager.CurrentContainerView != null && _scrollableManager.CurrentContainerViewPlacement != CurrentContainerViewPlacement.WithinList)
                    ResetMeasuredLengths(_scrollableManager.CurrentContainerView);
                foreach (var containerView in ContainerViewList)
                    ResetMeasuredLengths(containerView);
                InvalidateContainerLengths();
            }

            internal void RemoveFirst()
            {
                var first = _scrollableManager.ContainerViewList.First;
                ResetMeasuredLengths(first);
                _scrollableManager.ContainerViewList[1].StartOffset = 0;
                InvalidateContainerLengths();
            }

            internal void RemoveLast()
            {
                var last = _scrollableManager.ContainerViewList.Last;
                ResetMeasuredLengths(last);
                InvalidateContainerLengths();
            }

            private void ResetMeasuredLengths(ContainerView containerView)
            {
                if (containerView.CumulativeMeasuredLengths == null)
                    return;

                double totalLength = 0;
                var gridSpan = GridSpan;
                Debug.Assert(gridSpan.Count == containerView.CumulativeMeasuredLengths.Length);
                for (int i = 0; i < containerView.CumulativeMeasuredLengths.Length; i++)
                {
                    totalLength += GetInitialMeasuredLength(containerView, gridSpan[i]);
                    containerView.CumulativeMeasuredLengths[i] = totalLength;
                }

                containerView.StartOffset = 0;
            }

            private double GetInitialMeasuredLength(ContainerView containerView, GridTrack gridTrack)
            {
                var rowView = containerView as RowView;
                var length = rowView == null ? gridTrack.Length : _scrollableManager.GetLength(gridTrack, rowView.RowPresenter);
                return length.IsAuto ? 0 : length.Value;
            }

            public double SetMeasuredLength(ContainerView containerView, GridTrack gridTrack, double value)
            {
                Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
                value = Math.Max(gridTrack.MinLength, value);
                value = Math.Min(gridTrack.MaxLength, value);
                var oldValue = GetMeasuredLength(containerView, gridTrack);
                var delta = value - oldValue;
                if (delta == 0)
                    return 0;

                var index = gridTrack.VariantByContainerIndex;
                var cumulativeMeasuredLengths = GetCumulativeMeasuredLengths(containerView);
                for (int i = index; i < cumulativeMeasuredLengths.Length; i++)
                    cumulativeMeasuredLengths[i] += delta;
                InvalidateContainerLengths();
                return delta;
            }

            private double GetMeasuredLength(ContainerView containerView)
            {
                var cumulativeMeasuredLengths = GetCumulativeMeasuredLengths(containerView);
                return cumulativeMeasuredLengths[cumulativeMeasuredLengths.Length - 1];
            }

            public double GetMeasuredLength(ContainerView containerView, GridTrack gridTrack)
            {
                Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
                int index = gridTrack.VariantByContainerIndex;
                var cumulativeMeasuredLengths = GetCumulativeMeasuredLengths(containerView);
                return index == 0 ? cumulativeMeasuredLengths[0] : cumulativeMeasuredLengths[index] - cumulativeMeasuredLengths[index - 1];
            }

            private Span GetRelativeSpan(ContainerView containerView, GridTrack gridTrack)
            {
                Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
                return new Span(GetRelativeStartOffset(containerView, gridTrack), GetRelativeEndOffset(containerView, gridTrack));
            }

            private double GetRelativeStartOffset(ContainerView containerView, GridTrack gridTrack)
            {
                Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
                return GetRelativeEndOffset(containerView, gridTrack) - GetMeasuredLength(containerView, gridTrack);
            }

            private double GetRelativeEndOffset(ContainerView containerView, GridTrack gridTrack)
            {
                Debug.Assert(gridTrack != null && gridTrack.VariantByContainer);
                return GetCumulativeMeasuredLengths(containerView)[gridTrack.VariantByContainerIndex];
            }

            public Span GetRelativeSpan(GridTrack gridTrack, int ordinal)
            {
                Debug.Assert(ordinal >= 0 && ordinal < ContainerViewList.MaxCount);

                var containerView = ContainerViewList.GetContainerView(ordinal);
                return containerView != null ? GetRelativeSpan(containerView, gridTrack) : new Span(GetStartOffset(gridTrack), GetEndOffset(gridTrack));
            }

            private double GetStartOffset(GridTrack gridTrack)
            {
                RefreshContainerLengths();
                return gridTrack.VariantByContainerStartOffset;
            }

            private double GetEndOffset(GridTrack gridTrack)
            {
                return GetStartOffset(gridTrack) + gridTrack.VariantByContainerAvgLength;
            }

            public double GetContainerViewsLength(GridTrack gridTrack, int count)
            {
                Debug.Assert(count >= 0 && count <= ContainerViewList.MaxCount);
                if (count == 0)
                    return 0;

                var owner = gridTrack.Owner;

                var unrealized = ContainerViewList.Count == 0 ? 0 : ContainerViewList.First.ContainerOrdinal;
                if (count <= unrealized)
                    return count * AvgLength;

                var realized = ContainerViewList.Count == 0 ? 0 : ContainerViewList.Last.ContainerOrdinal - ContainerViewList.First.ContainerOrdinal + 1;
                if (count <= unrealized + realized)
                    return unrealized * AvgLength + GetRealizedContainersLength(count - unrealized);

                return GetRealizedContainersLength(realized) + (count - realized) * AvgLength;
            }

            private double GetRealizedContainersLength(int count)
            {
                Debug.Assert(count >= 0 && count <= ContainerViewList.Count);
                return count == 0 ? 0 : GetEndOffset(ContainerViewList[count - 1]);
            }
        }

        internal sealed override void VirtualizeFirst()
        {
            _variantLengthHandler?.RemoveFirst();
            base.VirtualizeFirst();
        }

        internal sealed override void VirtualizeLast()
        {
            _variantLengthHandler?.RemoveLast();
            base.VirtualizeLast();
        }
    }
}
