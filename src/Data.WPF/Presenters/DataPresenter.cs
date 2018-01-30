﻿using System.Collections.Generic;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System.Diagnostics;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public abstract class DataPresenter : IService
    {
        public event EventHandler ViewRefreshing = delegate { };
        public event EventHandler ViewRefreshed = delegate { };

        protected internal virtual void OnViewRefreshing()
        {
            ViewRefreshing(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewRefreshed()
        {
            ViewRefreshed(this, EventArgs.Empty);
        }

        public DataView View { get; private set; }

        public virtual void DetachView()
        {
            if (View == null)
                return;
            View.CleanupCommandEntries();
            View.DataPresenter = null;
            View = null;
        }

        internal void AttachView(DataView value)
        {
            if (View != null)
                DetachView();

            Debug.Assert(View == null && value != null);
            View = value;
            View.DataPresenter = this;
        }

        public event EventHandler<EventArgs> ViewChanged = delegate { };

        protected virtual void OnViewChanged()
        {
            CommandManager.InvalidateRequerySuggested();
            ViewChanged(this, EventArgs.Empty);
        }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager?.Template; }
        }

        public Orientation? LayoutOrientation
        {
            get { return Template?.Orientation; }
        }

        public DataSet DataSet
        {
            get { return LayoutManager?.DataSet; }
        }

        public bool IsRecursive
        {
            get { return LayoutManager == null ? false : LayoutManager.IsRecursive; }
        }

        private LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_NullDataSet);
            return LayoutManager;
        }

        public Predicate<DataRow> Where
        {
            get { return LayoutManager?.Where; }
            set { Apply(value, OrderBy); }
        }
        
        public IComparer<DataRow> OrderBy
        {
            get { return LayoutManager?.OrderBy; }
            set { Apply(Where, value); }
        }

        public virtual void Apply(Predicate<DataRow> where, IComparer<DataRow> orderBy)
        {
            RequireLayoutManager().Apply(where, orderBy);
        }

        public IReadOnlyList<RowPresenter> Rows
        {
            get { return LayoutManager?.Rows; }
        }

        public RowPresenter CurrentRow
        {
            get { return LayoutManager?.CurrentRow; }
            set
            {
                VerifyRowPresenter(value, nameof(value));
                RequireLayoutManager().CurrentRow = value;
            }
        }

        public ContainerView CurrentContainerView
        {
            get { return RequireLayoutManager().CurrentContainerView; }
        }

        public int FlowRepeatCount
        {
            get { return LayoutManager == null ? 1 : LayoutManager.FlowRepeatCount; }
        }

        private void VerifyRowPresenter(RowPresenter value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);

            if (value.DataPresenter != this)
                throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, paramName);
        }

        public void Select(RowPresenter rowPresenter, SelectionMode selectionMode, bool ensureVisible = true)
        {
            VerifyRowPresenter(rowPresenter, nameof(rowPresenter));
            var oldCurrentRow = CurrentRow;
            CurrentRow = rowPresenter;
            RequireLayoutManager().Select(rowPresenter, selectionMode, oldCurrentRow);
            if (ensureVisible && Scrollable != null)
                Scrollable.EnsureCurrentRowVisible();
        }

        public IReadOnlyCollection<RowPresenter> SelectedRows
        {
            get { return LayoutManager?.SelectedRows; }
        }

        public RowPresenter VirtualRow
        {
            get { return LayoutManager?.VirtualRow; }
        }

        public bool IsEditing
        {
            get { return LayoutManager == null ? false : LayoutManager.IsEditing; }
        }

        public RowPresenter EditingRow
        {
            get { return CurrentRow != null && IsEditing ? CurrentRow : null; }
        }

        public bool IsInserting
        {
            get { return IsEditing && LayoutManager.CurrentRow == LayoutManager.VirtualRow; }
        }

        public RowPresenter InsertingRow
        {
            get { return IsInserting ? CurrentRow : null; }
        }

        public bool CanInsert
        {
            get { return !IsEditing && RequireLayoutManager().DataSet.EditingRow == null; }
        }

        public void BeginInsertBefore(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertBefore(null, row);
        }

        public void BeginInsertAfter(RowPresenter row = null)
        {
            VerifyInsert(row);
            RequireLayoutManager().BeginInsertAfter(null, row);
        }

        private void VerifyInsert(RowPresenter row)
        {
            if (!CanInsert)
                throw new InvalidOperationException(DiagnosticMessages.DataPresenter_VerifyCanInsert);
            if (row != null)
            {
                if (row.DataPresenter != this || row.Parent != null)
                    throw new ArgumentException(DiagnosticMessages.DataPresenter_InvalidRowPresenter, nameof(row));
            }
        }

        public void InvalidateView()
        {
            RequireLayoutManager().InvalidateView();
        }

        public ScalarValidation ScalarValidation
        {
            get { return LayoutManager?.ScalarValidation; }
        }

        public RowValidation RowValidation
        {
            get { return LayoutManager?.RowValidation; }
        }

        public IScrollable Scrollable
        {
            get { return LayoutManager as ScrollableManager; }
        }

        internal protected virtual void ToggleExpandState(RowPresenter rowPresenter)
        {
            rowPresenter.InternalToggleExpandState();
        }

        internal protected virtual bool HasChildren(RowPresenter rowPresenter)
        {
            return rowPresenter.InternalHasChildren;
        }

        public virtual T GetService<T>()
            where T : class, IService
        {
            return (this is T) ? (T)((object)this) : ServiceManager.GetService<T>(this);
        }

        internal abstract void Reload();

        internal abstract bool CanReload { get; }

        internal abstract void CancelLoading();

        internal abstract bool CanCancelLoading { get; }

        private List<Scalar> _scalars = new List<Scalar>();
        public IReadOnlyList<Scalar> Scalars
        {
            get { return _scalars; }
        }

        protected Scalar<T> NewScalar<T>(T value = default(T))
        {
            var result = new Scalar<T>(value);
            _scalars.Add(result);
            return result;
        }

        internal IScalarValidationErrors ValidateScalars()
        {
            var result = ScalarValidationErrors.Empty;
            for (int i = 0; i < Scalars.Count; i++)
                result = Scalars[i].Validate(result);
            return ValidateScalars(result);
        }

        protected virtual IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
        {
            return result;
        }

        public bool HasVisibleInputError
        {
            get { return LayoutManager == null ? false : LayoutManager.HasVisibleError; }
        }

        public bool SubmitInput(bool focusToErrorInput = true)
        {
            RequireLayoutManager();

            RowValidation.Validate();
            ScalarValidation.Validate();
            if (HasVisibleInputError)
            {
                if (focusToErrorInput)
                    LayoutManager.FocusToInputError();
                return false;
            }

            if (IsEditing)
                CurrentRow.EndEdit();
            return true;
        }

        public void InvalidateMeasure()
        {
            RequireLayoutManager().InvalidateMeasure();
        }

        protected internal virtual void OnRowsLoaded(bool isReload)
        {
        }

        protected internal virtual void OnCurrentRowChanged(RowPresenter oldValue)
        {
        }

        protected internal virtual bool QueryEndEdit()
        {
            return RequireLayoutManager().QueryEndEdit();
        }

        protected internal virtual bool QueryCancelEdit()
        {
            return true;
        }

        #region IService

        DataPresenter IService.DataPresenter
        {
            get { return this; }
        }

        void IService.Initialize(DataPresenter dataPresenter)
        {
            throw new NotSupportedException();
        }
        #endregion

        protected internal virtual string FormatFaultMessage(AsyncValidator asyncValidator)
        {
            return AsyncValidationFault.FormatMessage(asyncValidator);
        }
    }
}
