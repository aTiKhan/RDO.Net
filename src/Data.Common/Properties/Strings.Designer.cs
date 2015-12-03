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
        /// The provided getter expression is invalid.
        /// </summary>
        public static string Accessor_InvalidGetter
        {
            get { return GetString("Accessor_InvalidGetter"); }
        }

        /// <summary>
        /// Cannot register accessor for type '{type}' after an instance of this type or its derived type has been created.
        /// </summary>
        public static string Accessor_RegisterAfterUse(object type)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Accessor_RegisterAfterUse", "type"), type);
        }

        /// <summary>
        /// The accessor with OwnerType '{ownerType}' and Name '{name}' has been registered already.
        /// </summary>
        public static string Accessor_RegisterDuplicate(object ownerType, object name)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Accessor_RegisterDuplicate", "ownerType", "name"), ownerType, name);
        }

        /// <summary>
        /// The argument '{argumentName}' cannot be null, empty or contain only white space.
        /// </summary>
        public static string ArgumentIsNullOrWhitespace(object argumentName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentIsNullOrWhitespace", "argumentName"), argumentName);
        }

        /// <summary>
        /// Cannot call When after Else has been called.
        /// </summary>
        public static string Case_WhenAfterElse
        {
            get { return GetString("Case_WhenAfterElse"); }
        }

        /// <summary>
        /// Cannot evaluate a non DataSet column: Column={column}, DataSource.Kind={dataSourceKind}.
        /// </summary>
        public static string ColumnAggregateFunction_EvalOnNonDataSet(object column, object dataSourceKind)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnAggregateFunction_EvalOnNonDataSet", "column", "dataSourceKind"), column, dataSourceKind);
        }

        /// <summary>
        /// Cannot resolve model chain from Column "{column}" to provided dataRow.
        /// </summary>
        public static string ColumnAggregateFunction_NoModelChain(object column)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnAggregateFunction_NoModelChain", "column"), column);
        }

        /// <summary>
        /// Duplicate ColumnKey is not allowed: OriginalOwnerType={originalOwnerType}, OriginalName={originalName}.
        /// </summary>
        public static string ColumnCollection_DuplicateColumnKey(object originalOwnerType, object originalName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnCollection_DuplicateColumnKey", "originalOwnerType", "originalName"), originalOwnerType, originalName);
        }

        /// <summary>
        /// The type resolver callback returns null.
        /// </summary>
        public static string GenericInvoker_TypeResolverReturnsNull
        {
            get { return GetString("GenericInvoker_TypeResolverReturnsNull"); }
        }

        /// <summary>
        /// The type "{type}" is not implemented correctly. The column getter "{columnGetter}" returns null.
        /// </summary>
        public static string ColumnGroup_GetterReturnsNull(object type, object columnGetter)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnGroup_GetterReturnsNull", "type", "columnGetter"), type, columnGetter);
        }

        /// <summary>
        /// The type "{type}" is not implemented correctly. The parent models of column "{column1}" and "{column2}" are inconsistent.
        /// </summary>
        public static string ColumnGroup_InconsistentParentModel(object type, object column1, object column2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnGroup_InconsistentParentModel", "type", "column1", "column2"), type, column1, column2);
        }

        /// <summary>
        /// The expression is already attached to a Column.
        /// </summary>
        public static string Column_ExpressionAlreadyAttached
        {
            get { return GetString("Column_ExpressionAlreadyAttached"); }
        }

        /// <summary>
        /// The expression cannot be set to a column owned by a model.
        /// </summary>
        public static string Column_ExpressionModelProperty
        {
            get { return GetString("Column_ExpressionModelProperty"); }
        }

        /// <summary>
        /// The column's Expression exists and cannot be overwrite.
        /// </summary>
        public static string Column_ExpressionOverwrite
        {
            get { return GetString("Column_ExpressionOverwrite"); }
        }

        /// <summary>
        /// The Model of the given DataRow must be the same as this column's ParentModel.
        /// </summary>
        public static string Column_VerifyDataRow
        {
            get { return GetString("Column_VerifyDataRow"); }
        }

        /// <summary>
        /// The Model of the given DbReader parameter must be the same as this column's ParentModel.
        /// </summary>
        public static string Column_VerifyDbReader
        {
            get { return GetString("Column_VerifyDbReader"); }
        }

        /// <summary>
        /// -- Canceled in {elapsedMilliSeconds} ms
        /// </summary>
        public static string DbLogger_CommandCanceled(object elapsedMilliSeconds)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_CommandCanceled", "elapsedMilliSeconds"), elapsedMilliSeconds);
        }

        /// <summary>
        /// -- Completed in {elapsedMilliSeconds} ms with result: {result}
        /// </summary>
        public static string DbLogger_CommandComplete(object elapsedMilliSeconds, object result)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_CommandComplete", "elapsedMilliSeconds", "result"), elapsedMilliSeconds, result);
        }

        /// <summary>
        /// -- Executing at {time}
        /// </summary>
        public static string DbLogger_CommandExecuting(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_CommandExecuting", "time"), time);
        }

        /// <summary>
        /// -- Executing asynchronously at {time}
        /// </summary>
        public static string DbLogger_CommandExecutingAsync(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_CommandExecutingAsync", "time"), time);
        }

        /// <summary>
        /// -- Failed in {elapsedMilliSeconds} ms with error: {error}
        /// </summary>
        public static string DbLogger_CommandFailed(object elapsedMilliSeconds, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_CommandFailed", "elapsedMilliSeconds", "error"), elapsedMilliSeconds, error);
        }

        /// <summary>
        /// Closed connection at {time}
        /// </summary>
        public static string DbLogger_ConnectionClosed(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionClosed", "time"), time);
        }

        /// <summary>
        /// Failed to close connection at {time} with error: {error}
        /// </summary>
        public static string DbLogger_ConnectionCloseError(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionCloseError", "time", "error"), time, error);
        }

        /// <summary>
        /// Opened connection at {time}
        /// </summary>
        public static string DbLogger_ConnectionOpen(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionOpen", "time"), time);
        }

        /// <summary>
        /// Opened connection asynchronously at {time}
        /// </summary>
        public static string DbLogger_ConnectionOpenAsync(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionOpenAsync", "time"), time);
        }

        /// <summary>
        /// Cancelled open connection at {time}
        /// </summary>
        public static string DbLogger_ConnectionOpenCanceled(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionOpenCanceled", "time"), time);
        }

        /// <summary>
        /// Failed to open connection at {time} with error: {error}
        /// </summary>
        public static string DbLogger_ConnectionOpenError(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionOpenError", "time", "error"), time, error);
        }

        /// <summary>
        /// Failed to open connection asynchronously at {time} with error: {error}
        /// </summary>
        public static string DbLogger_ConnectionOpenErrorAsync(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_ConnectionOpenErrorAsync", "time", "error"), time, error);
        }

        /// <summary>
        /// Failed to commit transaction at {time} with error: {error}
        /// </summary>
        public static string DbLogger_TransactionCommitError(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionCommitError", "time", "error"), time, error);
        }

        /// <summary>
        /// Committed transaction at {time}
        /// </summary>
        public static string DbLogger_TransactionCommitted(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionCommitted", "time"), time);
        }

        /// <summary>
        /// Failed to rollback transaction at {time} with error: {error}
        /// </summary>
        public static string DbLogger_TransactionRollbackError(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionRollbackError", "time", "error"), time, error);
        }

        /// <summary>
        /// Rolled back transaction at {time}
        /// </summary>
        public static string DbLogger_TransactionRolledBack(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionRolledBack", "time"), time);
        }

        /// <summary>
        /// Started transaction at {time}
        /// </summary>
        public static string DbLogger_TransactionStarted(object time)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionStarted", "time"), time);
        }

        /// <summary>
        /// Failed to start transaction at {time} with error: {error}
        /// </summary>
        public static string DbLogger_TransactionStartError(object time, object error)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbLogger_TransactionStartError", "time", "error"), time, error);
        }

        /// <summary>
        /// Cannot call From method multiple times.
        /// </summary>
        public static string DbQueryBuilder_DuplicateFrom
        {
            get { return GetString("DbQueryBuilder_DuplicateFrom"); }
        }

        /// <summary>
        /// Invalid left key. Its ParentModel must be previously added as source query.
        /// </summary>
        public static string DbQueryBuilder_Join_InvalidLeftKey
        {
            get { return GetString("DbQueryBuilder_Join_InvalidLeftKey"); }
        }

        /// <summary>
        /// Invalid right key. Its ParentModel must be the same as the provided model parameter.
        /// </summary>
        public static string DbQueryBuilder_Join_InvalidRightKey
        {
            get { return GetString("DbQueryBuilder_Join_InvalidRightKey"); }
        }

        /// <summary>
        /// Aggregate expression is not supported.
        /// </summary>
        public static string DbQueryBuilder_VerifySourceColumnAggregateModels
        {
            get { return GetString("DbQueryBuilder_VerifySourceColumnAggregateModels"); }
        }

        /// <summary>
        /// The expression contains parent model '{0}' which does not exist in the source queries.
        /// </summary>
        public static string DbQueryBuilder_VerifySourceColumnParentModels(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbQueryBuilder_VerifySourceColumnParentModels"), p0);
        }

        /// <summary>
        /// Invalid targetColumn. It must be a column of target model, and cannot be selected already.
        /// </summary>
        public static string DbQueryBuilder_VerifyTargetColumn
        {
            get { return GetString("DbQueryBuilder_VerifyTargetColumn"); }
        }

        /// <summary>
        /// The child model's ParentModel must be the same as this object's Model.
        /// </summary>
        public static string InvalidChildModel
        {
            get { return GetString("InvalidChildModel"); }
        }

        /// <summary>
        /// The child model returned by the getter is invalid. It cannot be null and its ParentModel must be the calling model.
        /// </summary>
        public static string InvalidChildModelGetter
        {
            get { return GetString("InvalidChildModelGetter"); }
        }

        /// <summary>
        /// The identity increment value cannot be 0.
        /// </summary>
        public static string Model_InvalidIdentityIncrement
        {
            get { return GetString("Model_InvalidIdentityIncrement"); }
        }

        /// <summary>
        /// The column must be child of this model.
        /// </summary>
        public static string Model_VerifyChildColumn
        {
            get { return GetString("Model_VerifyChildColumn"); }
        }

        /// <summary>
        /// The operation is only allowed in design mode.
        /// </summary>
        public static string VerifyDesignMode
        {
            get { return GetString("VerifyDesignMode"); }
        }

        /// <summary>
        /// Cannot define multiple identity column on the same table.
        /// </summary>
        public static string Model_MultipleIdentityColumn
        {
            get { return GetString("Model_MultipleIdentityColumn"); }
        }

        /// <summary>
        /// Cannot have more than one clustered index.  The clustered index '{existingClusterIndexName}' already exists.
        /// </summary>
        public static string Model_MultipleClusteredIndex(object existingClusterIndexName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Model_MultipleClusteredIndex", "existingClusterIndexName"), existingClusterIndexName);
        }

        /// <summary>
        /// The constraint '{constraintName}' already exists.
        /// </summary>
        public static string Model_DuplicateConstraintName(object constraintName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Model_DuplicateConstraintName", "constraintName"), constraintName);
        }

        /// <summary>
        /// The columns cannot be empty.
        /// </summary>
        public static string Model_EmptyColumns
        {
            get { return GetString("Model_EmptyColumns"); }
        }

        /// <summary>
        /// The reference table model is invalid. It must either be a self reference or a reference to a existing table.
        /// </summary>
        public static string Model_InvalidRefTableModel
        {
            get { return GetString("Model_InvalidRefTableModel"); }
        }

        /// <summary>
        /// The child DbSet has been created already.
        /// </summary>
        public static string DbSet_VerifyCreateChild_AlreadyCreated
        {
            get { return GetString("DbSet_VerifyCreateChild_AlreadyCreated"); }
        }

        /// <summary>
        /// Creating child DbSet on DbTable is not allowed.
        /// </summary>
        public static string DbSet_VerifyCreateChild_InvalidDataSourceKind
        {
            get { return GetString("DbSet_VerifyCreateChild_InvalidDataSourceKind"); }
        }

        /// <summary>
        /// Cannot access values of the column. The column must belong to a dataset.
        /// </summary>
        public static string Column_NullValueManager
        {
            get { return GetString("Column_NullValueManager"); }
        }

        /// <summary>
        /// Cannot set value of readonly column. The column is part of the primary key or child-parent relationship.
        /// </summary>
        public static string Column_SetReadOnlyValue
        {
            get { return GetString("Column_SetReadOnlyValue"); }
        }

        /// <summary>
        /// The column[{columnIndex}] '{column}' is not supported by this DbSession.
        /// </summary>
        public static string DbSession_ColumnNotSupported(object columnIndex, object column)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbSession_ColumnNotSupported", "columnIndex", "column"), columnIndex, column);
        }

        /// <summary>
        /// Reached EOF unexpectedly.
        /// </summary>
        public static string JsonParser_UnexpectedEof
        {
            get { return GetString("JsonParser_UnexpectedEof"); }
        }

        /// <summary>
        /// Invalid char '{ch}' at index {index}.
        /// </summary>
        public static string JsonParser_InvalidChar(object ch, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidChar", "ch", "index"), ch, index);
        }

        /// <summary>
        /// Char '{ch}' at index {index} is not a valid hex number.
        /// </summary>
        public static string JsonParser_InvalidHexChar(object ch, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidHexChar", "ch", "index"), ch, index);
        }

        /// <summary>
        /// '{ch}' expected at index {index}.
        /// </summary>
        public static string JsonParser_InvalidLiteral(object ch, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidLiteral", "ch", "index"), ch, index);
        }

        /// <summary>
        /// Invalid string escape '{stringEscape}' at index {index}.
        /// </summary>
        public static string JsonParser_InvalidStringEscape(object stringEscape, object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidStringEscape", "stringEscape", "index"), stringEscape, index);
        }

        /// <summary>
        /// Invalid member name "{memberName}" for Model "{model}".
        /// </summary>
        public static string JsonParser_InvalidModelMember(object memberName, object model)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidModelMember", "memberName", "model"), memberName, model);
        }

        /// <summary>
        /// Cannot deserialize from JSON value. Provided JSON value must be 'true', 'false' or 'null'.
        /// </summary>
        public static string BooleanColumn_CannotDeserialize
        {
            get { return GetString("BooleanColumn_CannotDeserialize"); }
        }

        /// <summary>
        /// Current token kind "{tokenKind}" is invalid. Expected token kind: {expectedTokenKind}.
        /// </summary>
        public static string JsonParser_InvalidTokenKind(object tokenKind, object expectedTokenKind)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("JsonParser_InvalidTokenKind", "tokenKind", "expectedTokenKind"), tokenKind, expectedTokenKind);
        }

        /// <summary>
        /// The source column derives from invalid model '{model}'.
        /// </summary>
        public static string ColumnMappingsBuilder_InvalidSourceParentModelSet(object model)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnMappingsBuilder_InvalidSourceParentModelSet", "model"), model);
        }

        /// <summary>
        /// The target column '{targetColumn}' is invalid.
        /// </summary>
        public static string ColumnMappingsBuilder_InvalidTarget(object targetColumn)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnMappingsBuilder_InvalidTarget", "targetColumn"), targetColumn);
        }

        /// <summary>
        /// Cannot match primary key of current table and the source Model.
        /// </summary>
        public static string DbTable_GetKeyMappings_CannotMatch
        {
            get { return GetString("DbTable_GetKeyMappings_CannotMatch"); }
        }

        /// <summary>
        /// The operation is not supported by readonly list.
        /// </summary>
        public static string NotSupportedByReadOnlyList
        {
            get { return GetString("NotSupportedByReadOnlyList"); }
        }

        /// <summary>
        /// Circular reference detected for table "{tableName}".
        /// </summary>
        public static string MockDb_CircularReference(object tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MockDb_CircularReference", "tableName"), tableName);
        }

        /// <summary>
        /// The table "{tableName}" cannot be mocked twice.
        /// </summary>
        public static string MockDb_DuplicateTable(object tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MockDb_DuplicateTable", "tableName"), tableName);
        }

        /// <summary>
        /// DbMock object cannot be initialized twice.
        /// </summary>
        public static string MockDb_InitializeTwice
        {
            get { return GetString("MockDb_InitializeTwice"); }
        }

        /// <summary>
        /// The mocking table is invalid. It must belong to the given DbSession.
        /// </summary>
        public static string MockDb_InvalidTable
        {
            get { return GetString("MockDb_InvalidTable"); }
        }

        /// <summary>
        /// Mock can only be called during initialization.
        /// </summary>
        public static string MockDb_MockOnlyAllowedDuringInitialization
        {
            get { return GetString("MockDb_MockOnlyAllowedDuringInitialization"); }
        }

        /// <summary>
        /// The type argument "{typeArgument}" does not match with type argument "{expectedTypeArgument}" used for for table "{tableName}".
        /// </summary>
        public static string MockDb_ModelTypeMismatch(object typeArgument, object expectedTypeArgument, object tableName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MockDb_ModelTypeMismatch", "typeArgument", "expectedTypeArgument", "tableName"), typeArgument, expectedTypeArgument, tableName);
        }

        /// <summary>
        /// The returned where expression is invalid.
        /// </summary>
        public static string DbTable_VerifyWhere
        {
            get { return GetString("DbTable_VerifyWhere"); }
        }

        /// <summary>
        /// The child column '{childColumn}' does not exist in the column mappings.
        /// </summary>
        public static string ChildColumnNotExistInColumnMappings(object childColumn)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ChildColumnNotExistInColumnMappings", "childColumn"), childColumn);
        }

        /// <summary>
        /// The operation requires a primary key of model '{model}'.
        /// </summary>
        public static string DbTable_NoPrimaryKey(object model)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DbTable_NoPrimaryKey", "model"), model);
        }

        /// <summary>
        /// The source column's data type '{sourceColumnDataType}' is invalid. Data type '{expectedDataType}' required.
        /// </summary>
        public static string ColumnMappingsBuilder_InvalidSourceDataType(object sourceColumnDataType, object expectedDataType)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnMappingsBuilder_InvalidSourceDataType", "sourceColumnDataType", "expectedDataType"), sourceColumnDataType, expectedDataType);
        }

        /// <summary>
        /// No ColumnMapping specified.
        /// </summary>
        public static string ColumnMappingsBuilder_NoColumnMapping
        {
            get { return GetString("ColumnMappingsBuilder_NoColumnMapping"); }
        }

        /// <summary>
        /// Invalid parent DataRow.
        /// </summary>
        public static string Column_InvalidParentDataRow
        {
            get { return GetString("Column_InvalidParentDataRow"); }
        }

        /// <summary>
        /// Cannot add DataRow into multiple DataSet.
        /// </summary>
        public static string DataSet_InvalidNewDataRow
        {
            get { return GetString("DataSet_InvalidNewDataRow"); }
        }

        /// <summary>
        /// Value is required for column '{column}'.
        /// </summary>
        public static string RequiredAttribute_DefaultErrorMessage(object column)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("RequiredAttribute_DefaultErrorMessage", "column"), column);
        }

        /// <summary>
        /// Cannot resolve static method of Func&lt;Column, DataRow, string&gt; from provided type '{funcType}' and method name '{funcName}'.
        /// </summary>
        public static string ColumnValidatorAttribute_InvalidMessageFunc(object funcType, object funcName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ColumnValidatorAttribute_InvalidMessageFunc", "funcType", "funcName"), funcType, funcName);
        }

        /// <summary>
        /// Target table must be a permanent table and has a identity column.
        /// </summary>
        public static string DbTable_VerifyUpdateIdentity
        {
            get { return GetString("DbTable_VerifyUpdateIdentity"); }
        }

        /// <summary>
        /// Delete is not supported for parent table.
        /// </summary>
        public static string DbTable_DeleteNotSupportedForParentTable
        {
            get { return GetString("DbTable_DeleteNotSupportedForParentTable"); }
        }

        /// <summary>
        /// The source is invalid: cross DbSession is not supported.
        /// </summary>
        public static string DbTable_InvalidDbSetSource
        {
            get { return GetString("DbTable_InvalidDbSetSource"); }
        }

        /// <summary>
        /// Char '{ch}' expected after '{prevInput}'.
        /// </summary>
        public static string DataRow_FromString_ExpectChar(object ch, object prevInput)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DataRow_FromString_ExpectChar", "ch", "prevInput"), ch, prevInput);
        }

        /// <summary>
        /// The child model name '{childModelName}' is invalid for DataRow '{dataRowPath}'.
        /// </summary>
        public static string DataRow_FromString_InvalidChildModelName(object childModelName, object dataRowPath)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DataRow_FromString_InvalidChildModelName", "childModelName", "dataRowPath"), childModelName, dataRowPath);
        }

        /// <summary>
        /// The DataRow ordinal '{dataRowOrdinal}' is invalid for DataSet '{dataSetPath}'.
        /// </summary>
        public static string DataRow_FromString_InvalidDataRowOrdinal(object dataRowOrdinal, object dataSetPath)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DataRow_FromString_InvalidDataRowOrdinal", "dataRowOrdinal", "dataSetPath"), dataRowOrdinal, dataSetPath);
        }

        /// <summary>
        /// Cannot parse string '{input}' into an integer value.
        /// </summary>
        public static string DataRow_FromString_ParseInt(object input)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("DataRow_FromString_ParseInt", "input"), input);
        }

        /// <summary>
        /// The accessor is invalid. It must be created by calling CreateAccessor method.
        /// </summary>
        public static string ScalarData_InvalidAccessor
        {
            get { return GetString("ScalarData_InvalidAccessor"); }
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
