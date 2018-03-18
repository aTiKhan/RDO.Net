﻿using DevZest.Data.Presenters.Primitives;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevZest.Data.Presenters;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;

namespace DevZest.Data.Views
{
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateSelected)]
    [TemplateVisualState(GroupName = VisualStates.GroupSelection, Name = VisualStates.StateUnselected)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateRegularRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateCurrentEditingRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewCurrentRow)]
    [TemplateVisualState(GroupName = VisualStates.GroupRowIndicator, Name = VisualStates.StateNewEditingRow)]
    public class RowHeader : ButtonBase, IRowElement, RowSelectionWiper.ISelector
    {
        public abstract class Styles
        {
            public static readonly StyleId Flat = new StyleId(typeof(RowHeader));
        }

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSelected), typeof(bool),
            typeof(RowHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));
        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(RowHeader), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility),
            typeof(RowHeader), new FrameworkPropertyMetadata(Visibility.Visible));

        static RowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RowHeader), new FrameworkPropertyMetadata(typeof(RowHeader)));
        }

        public RowHeader()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var rowPresenter = RowView.GetCurrent(this).RowPresenter;
            UpdateVisualStates(rowPresenter);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            private set { SetValue(IsSelectedPropertyKey, BooleanBoxes.Box(value)); }
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

        void IRowElement.Setup(RowPresenter p)
        {
            var dataPresenter = p.DataPresenter;
            RowSelectionWiper.EnsureSetup(dataPresenter);
        }

        void IRowElement.Refresh(RowPresenter p)
        {
            UpdateVisualStates(p);
        }

        void IRowElement.Cleanup(RowPresenter p)
        {
        }

        private void UpdateVisualStates(RowPresenter p)
        {
            UpdateVisualStates(p, true);
        }

        private void UpdateVisualStates(RowPresenter p, bool useTransitions)
        {
            if (!IsLoaded)
                return;

            if (p.IsSelected)
            {
                IsSelected = true;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateSelected);
            }
            else
            {
                IsSelected = false;
                VisualStates.GoToState(this, useTransitions, VisualStates.StateUnselected);
            }

            if (p.IsVirtual)
            {
                if (p.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewEditingRow);
                else if (p.IsCurrent)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewCurrentRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateNewRow);
            }
            else if (p.IsCurrent)
            {
                if (p.IsEditing)
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentEditingRow);
                else
                    VisualStates.GoToState(this, useTransitions, VisualStates.StateCurrentRow);
            }
            else
                VisualStates.GoToState(this, useTransitions, VisualStates.StateRegularRow);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Left);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
                HandleMouseButtonDown(MouseButton.Right);
            base.OnMouseRightButtonDown(e);
        }

        private RowPresenter RowPresenter
        {
            get { return this.GetRowPresenter(); }
        }

        private DataPresenter DataPresenter
        {
            get { return RowPresenter?.DataPresenter; }
        }

        private void HandleMouseButtonDown(MouseButton mouseButton)
        {
            var dataPresenter = DataPresenter;
            if (dataPresenter == null)
                return;

            dataPresenter.Select(RowPresenter, mouseButton, () =>
            {
                if (!IsKeyboardFocusWithin)
                    Focus();
            });
        }
    }
}
