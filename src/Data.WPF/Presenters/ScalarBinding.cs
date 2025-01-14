﻿using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class for scalar data binding.
    /// </summary>
    public abstract class ScalarBinding : TwoWayBinding, IConcatList<ScalarBinding>
    {
        #region IConcatList<ScalarBinding>

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ScalarBinding>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ScalarBinding IReadOnlyList<ScalarBinding>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IConcatList<ScalarBinding> IConcatList<ScalarBinding>.Sort(Comparison<ScalarBinding> comparision)
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ScalarBinding> IEnumerable<ScalarBinding>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        bool IConcatList<ScalarBinding>.IsSealed
        {
            get { return true; }
        }

        IConcatList<ScalarBinding> IConcatList<ScalarBinding>.Seal()
        {
            return this;
        }

        #endregion

        /// <summary>
        /// Gets the parent scalar binding.
        /// </summary>
        public ScalarBinding Parent { get; private set; }

        /// <inheritdoc/>
        public sealed override Binding ParentBinding
        {
            get { return Parent; }
        }

        internal void Seal(ScalarBinding parent, int ordinal)
        {
            Parent = parent;
            Ordinal = ordinal;
        }

        /// <summary>
        /// Gets the child bindings.
        /// </summary>
        public abstract IReadOnlyList<ScalarBinding> ChildBindings { get; }

        /// <summary>
        /// Gets the scalar input.
        /// </summary>
        public abstract Input<ScalarBinding, IScalars> ScalarInput { get; }

        internal abstract void BeginSetup(int startOffset, UIElement[] elements);

        internal abstract void BeginSetup(UIElement element);

        internal abstract UIElement Setup(int flowIndex);

        internal ScalarPresenter ScalarPresenter
        {
            get { return Template.ScalarPresenter; }
        }

        private bool _repeatsWhenFlow;
        /// <summary>
        /// Gets or sets the value indicates whether this binding should repeat when flow.
        /// </summary>
        [DefaultValue(false)]
        public bool RepeatsWhenFlow
        {
            get { return Parent != null ? Parent.RepeatsWhenFlow : _repeatsWhenFlow; }
            set
            {
                VerifyNotSealed();
                _repeatsWhenFlow = value;
            }
        }

        internal int CumulativeFlowRepeatCountDelta { get; set; }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_IntersectsWithRowRange(Ordinal));

            if (!RepeatsWhenFlow)
                return;

            if (Template.Flowable(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.Flowable(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_FlowRepeatableNotAllowedByTemplate(Ordinal));
        }

        private ElementManager ElementManager
        {
            get { return Template.ElementManager; }
        }

        /// <summary>
        /// Gets the flow repeat count.
        /// </summary>
        public int FlowRepeatCount
        {
            get { return Parent != null ? Parent.FlowRepeatCount : (RepeatsWhenFlow ? ElementManager.FlowRepeatCount : 1); }
        }

        internal abstract UIElement GetChild(UIElement parent, int index);

        /// <summary>
        /// Gets the view element at specified flow index.
        /// </summary>
        /// <param name="flowIndex">The flow index.</param>
        /// <returns>The view element.</returns>
        public UIElement this[int flowIndex]
        {
            get
            {
                if (Ordinal == -1)
                    return null;

                if (Parent != null)
                    return Parent.GetChild(Parent[flowIndex], Ordinal);

                if (flowIndex < 0 || flowIndex >= FlowRepeatCount)
                    throw new ArgumentOutOfRangeException(nameof(flowIndex));

                var ordinal = Ordinal;
                int prevCumulativeFlowRepeatCountDelta = ordinal == 0 ? 0 : Template.ScalarBindings[ordinal - 1].CumulativeFlowRepeatCountDelta;
                var elementIndex = ordinal * FlowRepeatCount - prevCumulativeFlowRepeatCountDelta + flowIndex;
                if (ordinal >= Template.ScalarBindingsSplit)
                {
                    elementIndex += ElementManager.ContainerViewList.Count;
                    if (ElementManager.IsCurrentContainerViewIsolated)
                        elementIndex++;
                }
                return ElementManager.Elements[elementIndex];
            }
        }

        internal override void VerifyFrozenMargins(string templateItemsName)
        {
            base.VerifyFrozenMargins(templateItemsName);
            if (LayoutOrientation == Orientation.Horizontal)
                VerifyHorizontalStretches();
            else
                VerifyVerticalStretches();
        }

        private Orientation LayoutOrientation
        {
            get { return Template.Orientation.Value; }
        }

        private void VerifyHorizontalStretches()
        {
            if (GridRange.HorizontallyIntersectsWith(Template.GridColumns.Count - Template.Stretches))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_InvalidStretches(Ordinal));
        }

        private void VerifyVerticalStretches()
        {
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.Stretches))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_InvalidStretches(Ordinal));
        }

        internal override AutoSizeWaiver CoercedAutoSizeWaiver
        {
            get
            {
                var result = base.CoercedAutoSizeWaiver;
                if (!Template.Orientation.HasValue)
                    return result;

                if (LayoutOrientation == Orientation.Horizontal)
                {
                    if (Template.RowRange.ColumnSpan.IntersectsWith(GridRange.ColumnSpan))
                        result |= AutoSizeWaiver.Width;
                }
                else
                {
                    if (Template.RowRange.RowSpan.IntersectsWith(GridRange.RowSpan))
                        result |= AutoSizeWaiver.Height;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicates whether this binding is editable.
        /// </summary>
        public bool IsEditable
        {
            get { return ScalarInput != null || ChildBindings.Any(x => x.IsEditable); }
        }
    }
}
