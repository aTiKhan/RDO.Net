// <auto-generated />
namespace DevZest.Data
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class DiagnosticMessages
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("DevZest.Data.DiagnosticMessages", typeof(DiagnosticMessages).GetTypeInfo().Assembly);

        /// <summary>
        /// The AsyncValidator is already initialized by a Template.
        /// </summary>
        public static string AsyncValidator_AlreadyInitialized
        {
            get { return GetString("AsyncValidator_AlreadyInitialized"); }
        }

        /// <summary>
        /// The Presenter is not mounted.
        /// </summary>
        public static string BasePresenter_NotMounted
        {
            get { return GetString("BasePresenter_NotMounted"); }
        }

        /// <summary>
        /// {typeArgName} must be an enum type.
        /// </summary>
        public static string BindingFactory_EnumTypeRequired(object typeArgName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BindingFactory_EnumTypeRequired", "typeArgName"), typeArgName);
        }

        /// <summary>
        /// Invalid input for type {type}.
        /// </summary>
        public static string BindingFactory_InvalidInput(object type)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BindingFactory_InvalidInput", "type"), type);
        }

        /// <summary>
        /// The composite view does not belong to this Binding.
        /// </summary>
        public static string Binding_InvalidCompositeView
        {
            get { return GetString("Binding_InvalidCompositeView"); }
        }

        /// <summary>
        /// The {frozen} value is invalid. It cuts across {bindings}[{ordinal}].
        /// </summary>
        public static string Binding_InvalidFrozenMargin(object frozen, object bindings, object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Binding_InvalidFrozenMargin", "frozen", "bindings", "ordinal"), frozen, bindings, ordinal);
        }

        /// <summary>
        /// The Binding has already been added.
        /// </summary>
        public static string Binding_VerifyAdding
        {
            get { return GetString("Binding_VerifyAdding"); }
        }

        /// <summary>
        /// The Binding is sealed and allows no modification.
        /// </summary>
        public static string Binding_VerifyNotSealed
        {
            get { return GetString("Binding_VerifyNotSealed"); }
        }

        /// <summary>
        /// BlockBindings[{index}] intersects with RowRange.
        /// </summary>
        public static string BlockBinding_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// BlockBinding is invalid when Template.Orientation is null.
        /// </summary>
        public static string BlockBinding_NullOrientation
        {
            get { return GetString("BlockBinding_NullOrientation"); }
        }

        /// <summary>
        /// BlockBindings[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string BlockBinding_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// BlockBindings[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string BlockBinding_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// Invalid column: the column must be a member of Model.
        /// </summary>
        public static string ColumnComparer_InvalidColumn
        {
            get { return GetString("ColumnComparer_InvalidColumn"); }
        }

        /// <summary>
        /// Invalid Model type, it must be type: {type}.
        /// </summary>
        public static string ColumnComparer_InvalidModelType(object type)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnComparer_InvalidModelType", "type"), type);
        }

        /// <summary>
        /// The CurrentRow is editing.
        /// </summary>
        public static string DataPresenter_CurrentRowIsEditing
        {
            get { return GetString("DataPresenter_CurrentRowIsEditing"); }
        }

        /// <summary>
        /// The DataView is invalid. It has been associated with another DataPresenter.
        /// </summary>
        public static string DataPresenter_InvalidDataView
        {
            get { return GetString("DataPresenter_InvalidDataView"); }
        }

        /// <summary>
        /// The RowPresenter is invalid for this DataPresenter or it's collapsed.
        /// </summary>
        public static string DataPresenter_InvalidRowPresenter
        {
            get { return GetString("DataPresenter_InvalidRowPresenter"); }
        }

        /// <summary>
        /// The CanInsert property must be true.
        /// </summary>
        public static string DataPresenter_VerifyCanInsert
        {
            get { return GetString("DataPresenter_VerifyCanInsert"); }
        }

        /// <summary>
        /// Selection operation is not allowed in edit mode.
        /// </summary>
        public static string GridCell_Presenter_SelectionNotAllowedInEditMode
        {
            get { return GetString("GridCell_Presenter_SelectionNotAllowedInEditMode"); }
        }

        /// <summary>
        /// The GridCell is not owned by this DataPresenter.
        /// </summary>
        public static string GridCell_Presenter_VerifyGridCell
        {
            get { return GetString("GridCell_Presenter_VerifyGridCell"); }
        }

        /// <summary>
        /// Auto width GridColumns[{ordinal}] is invalid for flowable layout.
        /// </summary>
        public static string GridColumn_InvalidAutoWidth(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridColumn_InvalidAutoWidth", "ordinal"), ordinal);
        }

        /// <summary>
        /// Star width GridColumns[{ordinal}] is invalid for horizontal or flowable layout.
        /// </summary>
        public static string GridColumn_InvalidStarWidth(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridColumn_InvalidStarWidth", "ordinal"), ordinal);
        }

        /// <summary>
        /// The input string "{input}" is invalid.
        /// </summary>
        public static string GridLengthParser_InvalidInput(object input)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridLengthParser_InvalidInput", "input"), input);
        }

        /// <summary>
        /// The value GridPlacement.Head is invalid for the end grid extent.
        /// </summary>
        public static string GridPlacement_InvalidHeadValue
        {
            get { return GetString("GridPlacement_InvalidHeadValue"); }
        }

        /// <summary>
        /// The value GridPlacement.Tail is invalid for the start grid extent.
        /// </summary>
        public static string GridPlacement_InvalidTailValue
        {
            get { return GetString("GridPlacement_InvalidTailValue"); }
        }

        /// <summary>
        /// The GridRange does not belong to the same Template.
        /// </summary>
        public static string GridRange_InvalidOwner
        {
            get { return GetString("GridRange_InvalidOwner"); }
        }

        /// <summary>
        /// Auto height GridRows[{ordinal}] is invalid for flowable layout.
        /// </summary>
        public static string GridRow_InvalidAutoHeight(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridRow_InvalidAutoHeight", "ordinal"), ordinal);
        }

        /// <summary>
        /// Star height GridRows[{ordinal}] is invalid for vertical or flowable layout.
        /// </summary>
        public static string GridRow_InvalidStarHeight(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridRow_InvalidStarHeight", "ordinal"), ordinal);
        }

        /// <summary>
        /// Must call BeginResize/EndResize in tandem.
        /// </summary>
        public static string GridTrack_EndResize_NotInTandem
        {
            get { return GetString("GridTrack_EndResize_NotInTandem"); }
        }

        /// <summary>
        /// Star length is invalid in {orientation} orientation layout.
        /// </summary>
        public static string GridTrack_Resize_InvalidStarLength(object orientation)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridTrack_Resize_InvalidStarLength", "orientation"), orientation);
        }

        /// <summary>
        /// The Input.Columns property is not a single Column.
        /// </summary>
        public static string InPlaceEditor_EditorRowBindingNotColumn
        {
            get { return GetString("InPlaceEditor_EditorRowBindingNotColumn"); }
        }

        /// <summary>
        /// The Input.Scalars property is not a single Scalar.
        /// </summary>
        public static string InPlaceEditor_EditorScalarBindingNotScalar
        {
            get { return GetString("InPlaceEditor_EditorScalarBindingNotScalar"); }
        }

        /// <summary>
        /// The editor binding is invalid. The Input property must not be null.
        /// </summary>
        public static string InPlaceEditor_VerifyEditorBinding
        {
            get { return GetString("InPlaceEditor_VerifyEditorBinding"); }
        }

        /// <summary>
        /// The trigger has already been initialized with another Input.
        /// </summary>
        public static string Input_TriggerAlreadyInitialized
        {
            get { return GetString("Input_TriggerAlreadyInitialized"); }
        }

        /// <summary>
        /// The expression must be static.
        /// </summary>
        public static string ModelExtensions_ExpressionMustBeStatic
        {
            get { return GetString("ModelExtensions_ExpressionMustBeStatic"); }
        }

        /// <summary>
        /// Placeholder '{name}' is null.
        /// </summary>
        public static string Pane_NullPalceholder(object name)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Pane_NullPalceholder", "name"), name);
        }

        /// <summary>
        /// Cannot find resource in the ResourceDictionary.
        /// </summary>
        public static string ResourceId_ResourceNotFound
        {
            get { return GetString("ResourceId_ResourceNotFound"); }
        }

        /// <summary>
        /// RowBindings[{index}] is out of the RowRange.
        /// </summary>
        public static string RowBinding_OutOfRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("RowBinding_OutOfRowRange", "index"), index);
        }

        /// <summary>
        /// Flush operation can only be performed on CurrentRow.
        /// </summary>
        public static string RowInput_FlushCurrentRowOnly
        {
            get { return GetString("RowInput_FlushCurrentRowOnly"); }
        }

        /// <summary>
        /// Change editing row is not allowed.
        /// </summary>
        public static string RowManager_ChangeEditingRowNotAllowed
        {
            get { return GetString("RowManager_ChangeEditingRowNotAllowed"); }
        }

        /// <summary>
        /// The parent DataRow is invalid.
        /// </summary>
        public static string RowMapper_InvalidParentDataRow
        {
            get { return GetString("RowMapper_InvalidParentDataRow"); }
        }

        /// <summary>
        /// Edit exclusive virtual row is not allowed.
        /// </summary>
        public static string RowPresenter_BeginEditExclusiveVirtual
        {
            get { return GetString("RowPresenter_BeginEditExclusiveVirtual"); }
        }

        /// <summary>
        /// The child row is invalid.
        /// </summary>
        public static string RowPresenter_InvalidChildRow
        {
            get { return GetString("RowPresenter_InvalidChildRow"); }
        }

        /// <summary>
        /// The GridTrack must be repeatable.
        /// </summary>
        public static string RowPresenter_Resize_InvalidGridTrack
        {
            get { return GetString("RowPresenter_Resize_InvalidGridTrack"); }
        }

        /// <summary>
        /// Star length is invalid for repeatable GridTrack.
        /// </summary>
        public static string RowPresenter_Resize_InvalidStarLength
        {
            get { return GetString("RowPresenter_Resize_InvalidStarLength"); }
        }

        /// <summary>
        /// Resize is only allowed with scrollable and non-flowable layout.
        /// </summary>
        public static string RowPresenter_Resize_NotAllowed
        {
            get { return GetString("RowPresenter_Resize_NotAllowed"); }
        }

        /// <summary>
        /// The column is invalid. It must belong to the DataSet, or be a valid extended column.
        /// </summary>
        public static string RowPresenter_VerifyColumn
        {
            get { return GetString("RowPresenter_VerifyColumn"); }
        }

        /// <summary>
        /// RowPresenter.IsCurrent must be true to allow this operation.
        /// </summary>
        public static string RowPresenter_VerifyIsCurrent
        {
            get { return GetString("RowPresenter_VerifyIsCurrent"); }
        }

        /// <summary>
        /// There is pending edit not completed.
        /// </summary>
        public static string RowPresenter_VerifyNoPendingEdit
        {
            get { return GetString("RowPresenter_VerifyNoPendingEdit"); }
        }

        /// <summary>
        /// FlowRepeatable ScalarBindings[{index}] is not allowed by Template(Template.FlowRepeatCount = 1).
        /// </summary>
        public static string ScalarBinding_FlowRepeatableNotAllowedByTemplate(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_FlowRepeatableNotAllowedByTemplate", "index"), index);
        }

        /// <summary>
        /// ScalarBindings[{index}] intersects with RowRange.
        /// </summary>
        public static string ScalarBinding_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// The Stretches value is invalid. It cuts across ScalarBindings[{ordinal}].
        /// </summary>
        public static string ScalarBinding_InvalidStretches(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_InvalidStretches", "ordinal"), ordinal);
        }

        /// <summary>
        /// Flowable ScalarBindings[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string ScalarBinding_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// Flowable ScalarBindings[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string ScalarBinding_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// The SuspendValueChangedNotification/ResumeValueChangedNotification must be called in tandem.
        /// </summary>
        public static string ScalarContainer_ResumeValueChangedWithoutSuspend
        {
            get { return GetString("ScalarContainer_ResumeValueChangedWithoutSuspend"); }
        }

        /// <summary>
        /// The source scalars cannot be empty.
        /// </summary>
        public static string ScalarValidationMessage_EmptySourceScalars
        {
            get { return GetString("ScalarValidationMessage_EmptySourceScalars"); }
        }

        /// <summary>
        /// The sortings data is invalid, it fails the validation.
        /// </summary>
        public static string Sorting_HasValidationError
        {
            get { return GetString("Sorting_HasValidationError"); }
        }

        /// <summary>
        /// Delimiter char cannot be double quotation mark.
        /// </summary>
        public static string TabularText_DelimiterCannotBeQuote
        {
            get { return GetString("TabularText_DelimiterCannotBeQuote"); }
        }

        /// <summary>
        /// Conflict between Template.ValidationScope and AsyncValidation.Scope detected.
        /// </summary>
        public static string TemplateBuilder_AsyncValidatorScopeConflict
        {
            get { return GetString("TemplateBuilder_AsyncValidatorScopeConflict"); }
        }

        /// <summary>
        /// The child model is invalid. It must be direct child model and has the same type.
        /// </summary>
        public static string TemplateBuilder_InvalidRecursiveChildModel
        {
            get { return GetString("TemplateBuilder_InvalidRecursiveChildModel"); }
        }

        /// <summary>
        /// The UIElement is already attached to Template.
        /// </summary>
        public static string Template_ElementAttachedAlready
        {
            get { return GetString("Template_ElementAttachedAlready"); }
        }

        /// <summary>
        /// The RowRange cannot be empty.
        /// </summary>
        public static string Template_EmptyRowRange
        {
            get { return GetString("Template_EmptyRowRange"); }
        }

        /// <summary>
        /// The {frozen} value is invalid. RowRange is always scrollable and cannot be frozen.
        /// </summary>
        public static string Template_InvalidFrozenMargin(object frozen)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Template_InvalidFrozenMargin", "frozen"), frozen);
        }

        /// <summary>
        /// The Stretches value must be less or equal to {frozen}.
        /// </summary>
        public static string Template_InvalidStretches(object frozen)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Template_InvalidStretches", "frozen"), frozen);
        }

        /// <summary>
        /// Input already exists.
        /// </summary>
        public static string TwoWayBinding_InputAlreadyExists
        {
            get { return GetString("TwoWayBinding_InputAlreadyExists"); }
        }

        /// <summary>
        /// The {array}[{index}] is null.
        /// </summary>
        public static string _ArgumentNullAtIndex(object array, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("_ArgumentNullAtIndex", "array", "index"), array, index);
        }

        /// <summary>
        /// The IsEditing property must be true.
        /// </summary>
        public static string _VerifyIsEditing
        {
            get { return GetString("_VerifyIsEditing"); }
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
