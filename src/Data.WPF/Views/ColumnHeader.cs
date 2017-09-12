﻿using DevZest.Data.Presenters;
using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Presenters.Services;
using DevZest.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateUnsorted)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortAscending)]
    [TemplateVisualState(GroupName = VisualStates.GroupSort, Name = VisualStates.StateSortDescending)]
    public class ColumnHeader : ButtonBase, IScalarElement
    {
        private sealed class DragHandler : DragHandlerBase
        {
            private ColumnHeader _columnHeader;

            public void BeginDrag(ColumnHeader columnHeader, UIElement resizeGripper, MouseEventArgs e)
            {
                _columnHeader = columnHeader;
                DragDetect(resizeGripper, e);
            }

            protected override void OnBeginDrag()
            {
            }

            protected override void OnDragDelta()
            {
                var binding = _columnHeader.GetBinding();
                var track = binding.GridRange.ColumnSpan.EndTrack;
                track.Length = new GridLength(track.MeasuredLength + MouseDeltaX, GridUnitType.Pixel);
            }

            protected override void OnEndDrag(UIElement dragElement, bool abort)
            {
            }
        }

        public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(nameof(CanSort), typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.True));

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SortDirection), typeof(SortDirection),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(SortDirection.Unspecified));

        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty IsResizeGripperProperty = DependencyProperty.RegisterAttached("IsResizeGripper", typeof(bool),
            typeof(ColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnHeader), new FrameworkPropertyMetadata(typeof(ColumnHeader)));
        }

        public ColumnHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(DataPresenter);
        }

        public Column Column { get; set; }

        public bool CanSort
        {
            get { return (bool)GetValue(CanSortProperty); }
            set { SetValue(CanSortProperty, BooleanBoxes.Box(value)); }
        }

        public SortDirection SortDirection
        {
            get { return (SortDirection)GetValue(SortDirectionProperty); }
            private set { SetValue(SortDirectionPropertyKey, value); }
        }

        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        public static bool GetIsResizeGripper(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsResizeGripperProperty);
        }

        public static void SetIsResizeGripper(DependencyObject obj, bool value)
        {
            obj.SetValue(IsResizeGripperProperty, BooleanBoxes.Box(value));
        }

        void IScalarElement.Cleanup(ScalarPresenter scalarPresenter)
        {
        }

        void IScalarElement.Refresh(ScalarPresenter scalarPresenter)
        {
            UpdateVisualState(scalarPresenter.DataPresenter);
        }

        private void UpdateVisualState(DataPresenter dataPresenter)
        {
            UpdateVisualState(dataPresenter, true);
        }

        private void UpdateVisualState(DataPresenter dataPresenter, bool useTransitions)
        {
            SortDirection = GetSortDirection(dataPresenter);
            if (!IsLoaded)  // First call of VisualStateManager.GotoState must after control loaded.
                return;
            if (SortDirection == SortDirection.Ascending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortAscending);
            else if (SortDirection == SortDirection.Descending)
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSortDescending);
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnsorted);
        }

        private DataPresenter DataPresenter
        {
            get { return DataView.GetCurrent(this)?.DataPresenter; }
        }

        private SortDirection GetSortDirection(DataPresenter dataPresenter)
        {
            if (!CanSort)
                return SortDirection.Unspecified;

            var orderBy = dataPresenter.GetService<ISortService>()?.OrderBy;
            if (orderBy == null || orderBy.Count == 0)
                return SortDirection.Unspecified;

            for (int i = 0; i < orderBy.Count; i++)
            {
                var columnComparer = orderBy[i];
                if (columnComparer.GetColumn(dataPresenter.DataSet.Model) == Column)
                    return columnComparer.Direction;
            }
            return SortDirection.Unspecified;
        }

        void IScalarElement.Setup(ScalarPresenter scalarPresenter)
        {
            var dataPresenter = scalarPresenter.DataPresenter;
            var commands = GetCommands(dataPresenter);
            EnsureCommandEntriesSetup(commands);
            commands.EnsureCommandEntriesSetup(dataPresenter.View);
        }

        private bool _commandEntriesSetup;
        private void EnsureCommandEntriesSetup(ColumnHeaderCommands commands)
        {
            if (_commandEntriesSetup)
                return;

            this.SetupCommandEntries(commands.GetCommandEntries(this));
            _commandEntriesSetup = true;
        }

        private static ColumnHeaderCommands GetCommands(DataPresenter dataPresenter)
        {
            Debug.Assert(dataPresenter != null);
            return dataPresenter.GetService<ColumnHeaderCommands>(() => new ColumnHeaderCommands());
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.Handled)
                return;

            var resizeGripper = e.OriginalSource as UIElement;
            if (resizeGripper == null || !GetIsResizeGripper(resizeGripper))
                return;

            if (e.ClickCount == 1)
            {
                new DragHandler().BeginDrag(this, resizeGripper, e);
                e.Handled = true;
            }
            else if (e.ClickCount == 2)
            {
                throw new NotImplementedException();
                e.Handled = true;
            }
        }
    }
}
