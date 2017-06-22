// <auto-generated />
namespace DevZest.Data
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Strings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("DevZest.Data.Strings", typeof(Strings).GetTypeInfo().Assembly);

        /// <summary>
        /// The {array}[{index}] is null.
        /// </summary>
        public static string ArgumentNullAtIndex(object array, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentNullAtIndex", "array", "index"), array, index);
        }

        /// <summary>
        /// Invalid input for type {type}.
        /// </summary>
        public static string BindingFactory_InvalidInput(object type)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BindingFactory_InvalidInput", "type"), type);
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
        /// The RowPresenter is invalid for this DataPresenter.
        /// </summary>
        public static string DataPresenter_InvalidRowPresenter
        {
            get { return GetString("DataPresenter_InvalidRowPresenter"); }
        }

        /// <summary>
        /// The DataPresenter is not initialized: the DataSet property is null.
        /// </summary>
        public static string DataPresenter_NullDataSet
        {
            get { return GetString("DataPresenter_NullDataSet"); }
        }

        /// <summary>
        /// The CanInsert property must be true.
        /// </summary>
        public static string DataPresenter_VerifyCanInsert
        {
            get { return GetString("DataPresenter_VerifyCanInsert"); }
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
        /// The RowBinding must be added into Template before adding async validator.
        /// </summary>
        public static string RowInput_NullTemplateWhenAddAsyncValidator
        {
            get { return GetString("RowInput_NullTemplateWhenAddAsyncValidator"); }
        }

        /// <summary>
        /// Change current row is not allowed (CanChangeCurrentRow property is false).
        /// </summary>
        public static string RowManager_ChangeCurrentRowNotAllowed
        {
            get { return GetString("RowManager_ChangeCurrentRowNotAllowed"); }
        }

        /// <summary>
        /// The parent DataRow is invalid.
        /// </summary>
        public static string RowMapper_InvalidParentDataRow
        {
            get { return GetString("RowMapper_InvalidParentDataRow"); }
        }

        /// <summary>
        /// The virtual row cannot be deleted.
        /// </summary>
        public static string RowPresenter_DeleteVirtualRow
        {
            get { return GetString("RowPresenter_DeleteVirtualRow"); }
        }

        /// <summary>
        /// The child row is invalid.
        /// </summary>
        public static string RowPresenter_InvalidChildRow
        {
            get { return GetString("RowPresenter_InvalidChildRow"); }
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
        /// The IsEditing property must be true.
        /// </summary>
        public static string RowPresenter_VerifyIsEditing
        {
            get { return GetString("RowPresenter_VerifyIsEditing"); }
        }

        /// <summary>
        /// There is pending edit not completed.
        /// </summary>
        public static string RowPresenter_VerifyNoPendingEdit
        {
            get { return GetString("RowPresenter_VerifyNoPendingEdit"); }
        }

        /// <summary>
        /// Flowable ScalarBindings[{index}] is not allowed by Template(Template.FlowCount = 1).
        /// </summary>
        public static string ScalarBinding_FlowableNotAllowedByTemplate(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_FlowableNotAllowedByTemplate", "index"), index);
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
