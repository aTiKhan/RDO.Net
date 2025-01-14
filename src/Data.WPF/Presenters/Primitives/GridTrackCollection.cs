﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    internal abstract class GridTrackCollection<T> : ReadOnlyCollection<T>, IGridTrackCollection
        where T : GridTrack, IConcatList<T>
    {
        internal GridTrackCollection(Template template)
            : base(new List<T>())
        {
            _template = template;
        }

        private readonly Template _template;
        public Template Template
        {
            get { return _template; }
        }

        public abstract Orientation Orientation { get; }

        protected abstract GridSpan<T> GetGridSpan(GridRange gridRange);

        GridSpan IGridTrackCollection.GetGridSpan(GridRange gridRange)
        {
            var result = GetGridSpan(gridRange);
            return new GridSpan(result.StartTrack, result.EndTrack);
        }

        public void Add(T item)
        {
            Items.Add(item);
            AddLength(item.Length);
        }

        private void AddLength(GridLength length)
        {
            if (length.IsAbsolute)
                TotalAbsoluteLength += length.Value;
            else if (length.IsStar)
                TotalStarFactor += length.Value;
        }

        private void RemoveLength(GridLength length)
        {
            if (length.IsAbsolute)
                TotalAbsoluteLength -= length.Value;
            else if (length.IsStar)
                TotalStarFactor -= length.Value;
        }

        public double TotalAbsoluteLength { get; private set; }

        public double TotalAutoLength { get; set; }

        public double TotalStarFactor { get; private set; }

        public virtual void OnResized(GridTrack gridTrack, GridLength oldValue)
        {
            Debug.Assert(gridTrack.Owner == this);
            RemoveLength(oldValue);
            AddLength(gridTrack.Length);
        }

        private IConcatList<T> Filter(Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(GridSpan<T>.From(this), predict, action);
        }

        public IConcatList<T> Filter(GridRange gridRange, Func<T, bool> predict, Action<T> action = null)
        {
            return Filter(GetGridSpan(gridRange), predict, action);
        }

        private IConcatList<T> Filter(GridSpan<T> gridSpan, Func<T, bool> predict, Action<T> action = null)
        {
            var result = ConcatList<T>.Empty;
            if (gridSpan.IsEmpty)
                return result;

            var startIndex = gridSpan.StartTrack.Ordinal;
            var endIndex = gridSpan.EndTrack.Ordinal;
            for (int i = startIndex; i <= endIndex; i++)
            {
                var track = this[i];
                if (predict(track))
                    result = result.Concat(track);
                if (action != null)
                    action(track);
            }
            return result;
        }

        private bool KeepMeasuredAutoLength
        {
            get
            {
                var templateOrientation = Template.Orientation;
                return templateOrientation.HasValue ? templateOrientation != Orientation : false;
            }
        }

        internal void InitMeasuredLengths()
        {
            TotalAutoLength = 0;
            foreach (var gridTrack in this)
            {
                if (gridTrack.VariantByContainer)
                    continue;

                if (gridTrack.IsAutoLength)
                {
                    if (!KeepMeasuredAutoLength)
                        gridTrack.SetMeasuredLength(0);
                    TotalAutoLength += gridTrack.MeasuredLength;
                }
                else if (gridTrack.IsStarLength)
                    gridTrack.SetMeasuredLength(0);
                else
                    gridTrack.SetMeasuredLength(gridTrack.Length.Value);
            }
        }

        public double GetMeasuredLength(GridRange gridRange)
        {
            return GetMeasuredLength(GetGridSpan(gridRange));
        }

        internal double GetMeasuredLength(GridSpan<T> gridSpan)
        {
            if (gridSpan.IsEmpty)
                return 0;

            var startOrdinal = gridSpan.StartTrack.Ordinal;
            var endOrdinal = gridSpan.EndTrack.Ordinal;
            Debug.Assert(startOrdinal >= 0 && startOrdinal <= endOrdinal && endOrdinal < Count);
            return this[endOrdinal].EndOffset - this[startOrdinal].StartOffset;
        }

        internal double GetMeasuredLength(GridSpan<T> gridSpan, Func<T, bool> predict)
        {
            if (gridSpan.IsEmpty)
                return 0;

            var startOrdinal = gridSpan.StartTrack.Ordinal;
            var endOrdinal = gridSpan.EndTrack.Ordinal;
            Debug.Assert(startOrdinal >= 0 && startOrdinal <= endOrdinal && endOrdinal < Count);
            double result = 0d;
            for (int i = startOrdinal; i <= endOrdinal; i++)
            {
                var track = this[i];
                if (predict == null || predict(track))
                    result += track.MeasuredLength;
            }
            return result;
        }

        private bool _isOffsetValid = true;

        void IGridTrackCollection.InvalidateOffset()
        {
            _isOffsetValid = false;
        }

        void IGridTrackCollection.RefreshOffset()
        {
            if (_isOffsetValid)
                return;

            _isOffsetValid = true;  // prevent re-entrance
            for (int i = 1; i < Count; i++)
            {
                var current = this[i];
                if (current.VariantByContainer)
                    continue;
                var prev = this[i - 1].VariantByContainerExcluded;
                if (prev == null)
                    continue;
                current.StartOffset = prev.EndOffset;
            }
        }

        GridTrack IReadOnlyList<GridTrack>.this[int index]
        {
            get { return this[index]; }
        }

        IEnumerator<GridTrack> IEnumerable<GridTrack>.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IConcatList<T> _starLengthTracks;
        private IConcatList<T> StarLengthTracks
        {
            get
            {
                if (_starLengthTracks == null)
                    _starLengthTracks = Filter(x => x.IsStarLength);
                return _starLengthTracks;
            }
        }

        public void InvalidateStarLengthTracks()
        {
            _starLengthTracks = null;
        }

        public void DistributeStarLengths()
        {
            if (StarLengthTracks.Count == 0)
                return;

            var totalLength = Math.Max(0d, AvailableLength - TotalAbsoluteLength - TotalAutoLength);
            var totalStarFactor = TotalStarFactor;
            foreach (var gridTrack in StarLengthTracks)
                gridTrack.SetMeasuredLength(totalLength * (gridTrack.Length.Value / totalStarFactor));
        }

        private GridSpan<T> ContainerSpan
        {
            get { return GetGridSpan(Template.ContainerRange); }
        }

        private GridSpan<T> RowSpan
        {
            get { return GetGridSpan(Template.RowRange); }
        }

        public abstract int FrozenHeadTracksCount { get; }

        protected abstract string FrozenHeadName { get; }

        public abstract int FrozenTailTracksCount { get; }

        protected abstract string FrozenTailName { get; }

        public int HeadTracksCount
        {
            get { return ContainerStart.Ordinal; }
        }

        public int ContainerTracksCount
        {
            get { return ContainerEnd.Ordinal - ContainerStart.Ordinal + 1; }
        }

        public int RowTracksCount
        {
            get { return RowEnd.Ordinal - RowStart.Ordinal + 1; }
        }

        public int TailTracksCount
        {
            get { return Count - 1 - ContainerEnd.Ordinal; }
        }

        private int Stretches
        {
            get { return Template.Stretches; }
        }

        internal void VerifyFrozenMargins()
        {
            if (FrozenHeadTracksCount > HeadTracksCount)
                throw new InvalidOperationException(DiagnosticMessages.Template_InvalidFrozenMargin(FrozenHeadName));
            if (FrozenTailTracksCount > TailTracksCount)
                throw new InvalidOperationException(DiagnosticMessages.Template_InvalidFrozenMargin(FrozenTailName));
            if (Stretches > FrozenTailTracksCount)
                throw new InvalidOperationException(DiagnosticMessages.Template_InvalidStretches(FrozenTailName));
        }

        public GridTrack ContainerStart
        {
            get { return ContainerSpan.StartTrack; }
        }

        public GridTrack ContainerEnd
        {
            get { return ContainerSpan.EndTrack; }
        }

        public GridTrack RowStart
        {
            get { return RowSpan.StartTrack; }
        }

        public GridTrack RowEnd
        {
            get { return RowSpan.EndTrack; }
        }

        public abstract bool SizeToContent { get; }

        public abstract double AvailableLength { get; }

        public abstract Vector ToVector(double valueMain, double valueCross);

        public ScrollableManager ScrollableManager
        {
            get { return Template.ScrollableManager; }
        }

        public bool VariantByContainer
        {
            get
            {
                var scrollableManager = ScrollableManager;
                if (scrollableManager == null || scrollableManager.GridTracksMain != this)
                    return false;

                return scrollableManager.IsContainerLengthVariant;
            }
        }
    }
}
