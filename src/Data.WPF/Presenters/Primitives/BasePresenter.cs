﻿using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class BasePresenter : ScalarContainer.IOwner
    {
        public enum MountMode
        {
            Show,
            Refresh,
            Reload
        }

        public sealed class MountEventArgs
        {
            public static readonly MountEventArgs Show = new MountEventArgs(MountMode.Show);
            public static readonly MountEventArgs Refresh = new MountEventArgs(MountMode.Refresh);
            public static readonly MountEventArgs Reload = new MountEventArgs(MountMode.Reload);

            public static MountEventArgs Select(MountMode mode)
            {
                if (mode == MountMode.Show)
                    return Show;
                else if (mode == MountMode.Refresh)
                    return Refresh;
                else if (mode == MountMode.Reload)
                    return Reload;
                else
                    return null;
            }

            private MountEventArgs(MountMode mode)
            {
                Mode = mode;
            }

            public MountMode Mode { get; private set; }
        }

        protected BasePresenter()
        {
            _scalarContainer = new ScalarContainer(this);
        }

        public event EventHandler ViewInvalidating = delegate { };
        public event EventHandler ViewInvalidated = delegate { };
        public event EventHandler ViewRefreshing = delegate { };
        public event EventHandler ViewRefreshed = delegate { };

        internal IBaseView View
        {
            get { return GetView(); }
            set { SetView(value); }
        }

        internal abstract IBaseView GetView();

        internal abstract void SetView(IBaseView value);

        public virtual void DetachView()
        {
            if (View == null)
                return;
            Template.UnmountAttachedScalarBindings();
            View.Presenter = null;
            View = null;
        }

        internal void AttachView(IBaseView value)
        {
            if (View != null)
                DetachView();

            Debug.Assert(View == null && value != null);
            View = value;
            View.Presenter = this;
        }

        public event EventHandler<EventArgs> ViewChanged = delegate { };

        protected virtual void OnViewChanged()
        {
            CommandManager.InvalidateRequerySuggested();
            ViewChanged(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewInvalidating()
        {
            ViewInvalidating(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewInvalidated()
        {
            ViewInvalidated(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewRefreshing()
        {
            ViewRefreshing(this, EventArgs.Empty);
        }

        protected internal virtual void OnViewRefreshed()
        {
            ViewRefreshed(this, EventArgs.Empty);
        }

        public bool IsMounted
        {
            get { return LayoutManager != null; }
        }

        public event EventHandler<MountEventArgs> Mounted = delegate { };

        internal void OnMounted(MountMode mode)
        {
            Template.MountAttachedScalarBindings();
            IsAttachedScalarBindingsInvalidated = true;
            OnMounted(MountEventArgs.Select(mode));
        }

        internal bool IsAttachedScalarBindingsInvalidated { get; private set; }

        internal void ResetIsAttachedScalarBindingsInvalidated()
        {
            IsAttachedScalarBindingsInvalidated = false;
        }

        protected virtual void OnMounted(MountEventArgs e)
        {
            Mounted(this, e);
        }

        internal abstract LayoutManager LayoutManager { get; }

        public Template Template
        {
            get { return LayoutManager?.Template; }
        }

        internal LayoutManager RequireLayoutManager()
        {
            if (LayoutManager == null)
                throw new InvalidOperationException(DiagnosticMessages.CommonPresenter_NotMounted);
            return LayoutManager;
        }

        private readonly ScalarContainer _scalarContainer;
        public ScalarContainer ScalarContainer
        {
            get { return _scalarContainer; }
        }

        public void InvalidateView()
        {
            LayoutManager?.InvalidateView();
        }

        public void SuspendInvalidateView()
        {
            RequireLayoutManager().SuspendInvalidateView();
        }

        public void ResumeInvalidateView()
        {
            RequireLayoutManager().ResumeInvalidateView();
        }

        public ScalarValidation ScalarValidation
        {
            get { return LayoutManager?.ScalarValidation; }
        }

        protected Scalar<T> NewScalar<T>(T value = default(T), IEqualityComparer<T> equalityComparer = null)
        {
            return ScalarContainer.CreateNew(value, equalityComparer);
        }

        protected Scalar<T> NewLinkedScalar<T>(string propertyOrFieldName, IEqualityComparer<T> equalityComparer = null)
        {
            var getter = this.GetPropertyOrFieldGetter<T>(propertyOrFieldName);
            var setter = this.GetPropertyOrFieldSetter<T>(propertyOrFieldName);
            return ScalarContainer.CreateNew(getter, setter, equalityComparer);
        }

        protected Scalar<T> NewLinkedScalar<T>(Func<T> getter, Action<T> setter, IEqualityComparer<T> equalityComparer = null)
        {
            getter.VerifyNotNull(nameof(getter));
            setter.VerifyNotNull(nameof(setter));
            return ScalarContainer.CreateNew(getter, setter, equalityComparer);
        }

        internal IScalarValidationErrors ValidateScalars()
        {
            var result = ScalarValidationErrors.Empty;
            for (int i = 0; i < ScalarContainer.Count; i++)
                result = ScalarContainer[i].Validate(result);
            return ValidateScalars(result);
        }

        protected virtual IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
        {
            return result;
        }

        public bool CanSubmitInput
        {
            get { return LayoutManager == null ? false : LayoutManager.CanSubmitInput; }
        }

        public virtual bool SubmitInput(bool focusToErrorInput = true)
        {
            RequireLayoutManager();

            ScalarValidation.Validate();
            if (!CanSubmitInput)
            {
                if (focusToErrorInput)
                    LayoutManager.FocusToInputError();
                return false;
            }

            return true;
        }

        public void InvalidateMeasure()
        {
            RequireLayoutManager().InvalidateMeasure();
        }

        protected virtual void OnValueChanged(IScalars scalars)
        {
        }

        protected internal virtual bool QueryEndEditScalars()
        {
            return RequireLayoutManager().QueryEndEditScalars();
        }

        protected internal virtual bool ConfirmEndEditScalars()
        {
            return true;
        }

        void ScalarContainer.IOwner.OnValueChanged(IScalars scalars)
        {
            OnValueChanged(scalars);
        }

        bool ScalarContainer.IOwner.QueryEndEdit()
        {
            return QueryEndEditScalars();
        }

        void ScalarContainer.IOwner.OnBeginEdit()
        {
            ScalarValidation.EnterEdit();
        }

        void ScalarContainer.IOwner.OnCancelEdit()
        {
            ScalarValidation.CancelEdit();
        }

        void ScalarContainer.IOwner.OnEndEdit()
        {
            ScalarValidation.ExitEdit();
        }

        void ScalarContainer.IOwner.OnEdit(Scalar scalar)
        {
            OnEdit(scalar);
        }

        protected virtual void OnEdit(Scalar scalar)
        {
        }

        protected internal virtual string FormatFaultMessage(AsyncValidator asyncValidator)
        {
            return AsyncValidationFault.FormatMessage(asyncValidator);
        }
    }
}
