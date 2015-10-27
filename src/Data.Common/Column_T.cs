﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a column with strongly typed data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    public abstract class Column<T> : Column, IColumn
    {
        private interface IValueManager
        {
            int RowCount { get; }
            bool IsVirtual { get; }
            bool ShouldSerialize { get; }
            bool IsReadOnly(int ordinal);
            T this[int ordinal] { get; set; }
            void AddRow(DataRow dataRow);
            void RemoveRow(DataRow dataRow);
            void ClearRows();
        }

        private sealed class ChildValueManager : IValueManager
        {
            public ChildValueManager(IDataSet dataSet, Column<T> parentColumn)
            {
                Debug.Assert(dataSet != null);
                Debug.Assert(parentColumn != null);
                Debug.Assert(dataSet.Model.ParentModel == parentColumn.ParentModel);
                _dataSet = dataSet;
                _parentColumn = parentColumn;
            }

            private IDataSet _dataSet;
            private Column<T> _parentColumn;

            public int RowCount
            {
                get { return _dataSet.Count; }
            }

            public bool IsVirtual
            {
                get { return true; }
            }

            public bool ShouldSerialize
            {
                get { return false; }
            }

            public bool IsReadOnly(int ordinal)
            {
                return true;
            }

            public T this[int ordinal]
            {
                get { return _parentColumn[_dataSet[ordinal].Parent.Ordinal]; }
                set { Debug.Fail("Column in child relationship is readonly."); }
            }

            public void AddRow(DataRow dataRow)
            {
            }

            public void RemoveRow(DataRow dataRow)
            {
            }

            public void ClearRows()
            {
            }
        }

        private sealed class ListValueManager : IValueManager
        {
            public ListValueManager(Column<T> column)
            {
                Debug.Assert(column != null);
                _column = column;
                _isPrimaryKey = GetIsPrimaryKey();
            }

            private List<T> _values = new List<T>();
            private Column<T> _column;

            bool _isPrimaryKey;

            private bool GetIsPrimaryKey()
            {
                var primaryKey = Model.PrimaryKey;
                if (primaryKey == null)
                    return false;

                for (int i = 0; i < primaryKey.Count; i++)
                {
                    if (_column == primaryKey[i].Column)
                        return true;
                }
                return false;
            }

            private Model Model
            {
                get { return _column.ParentModel; }
            }

            public int RowCount
            {
                get { return _values.Count; }
            }

            public bool IsVirtual
            {
                get { return false; }
            }

            public bool ShouldSerialize
            {
                get { return true; }
            }

            public bool IsReadOnly(int ordinal)
            {
                if (Model.IsKeyUpdateAllowed)
                    return false;
                if (_isPrimaryKey)
                    return !_column.IsNull(_values[ordinal]);
                return false;
            }

            public T this[int ordinal]
            {
                get { return _values[ordinal]; }
                set { _values[ordinal] = value; }
            }

            public void AddRow(DataRow dataRow)
            {
                Debug.Assert(Model == dataRow.Model);
                _values.Insert(dataRow.Ordinal, _column.GetDefaultValue(dataRow));
            }

            public void RemoveRow(DataRow dataRow)
            {
                Debug.Assert(dataRow != null);
                Debug.Assert(dataRow.Model == Model);
                _values.RemoveAt(dataRow.Ordinal);
            }

            public void ClearRows()
            {
                _values.Clear();
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Column{T}"/> object.
        /// </summary>
        protected Column()
        {
        }

        internal sealed override void Seal()
        {
            base.Seal();

            var dataSet = ParentModel.DataSet;
            if (dataSet != null)
                _valueManager = CreateValueManager(dataSet, this);
        }

        private static IValueManager CreateValueManager(IDataSet dataSet, Column<T> column)
        {
            var parentColumn = GetParentColumn(column);
            if (parentColumn != null)
                return new ChildValueManager(dataSet, parentColumn);
            else
                return new ListValueManager(column);
        }

        private static Column<T> GetParentColumn(Column<T> column)
        {
            var model = column.ParentModel;
            var columnMappings = model.ParentModelColumnMappings;
            if (columnMappings == null)
                return null;
            foreach (var columnMapping in columnMappings)
            {
                if (columnMapping.Source == column)
                    return (Column<T>)columnMapping.Target;
            }
            return null;
        }

        private IValueManager _valueManager;

        private IValueManager ValueManager
        {
            get
            {
                if (_valueManager == null)
                    throw new InvalidOperationException(Strings.Column_NullValueManager);
                return _valueManager;
            }
        }

        private ColumnExpression<T> _expression;
        /// <summary>Gets the expression of this column.</summary>
        /// <value>The expression of this column.</value>
        public ColumnExpression<T> Expression
        {
            get { return _expression; }
            internal set
            {
                Check.NotNull(value, nameof(value));
                if (value.Owner != null)
                    throw new ArgumentException(Strings.Column_ExpressionAlreadyAttached, nameof(value));
                if (_expression != null)
                    throw new InvalidOperationException(Strings.Column_ExpressionOverwrite);
                if (ParentModel != null)
                    throw new InvalidOperationException(Strings.Column_ExpressionModelProperty);

                value.Owner = this;
                _expression = value;
            }
        }

        /// <summary>Gets or sets the value of this column from provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The provided <see cref="DataRow"/> object.</param>
        /// <returns>The value of this column from provided <see cref="DataRow"/> object.</returns>
        /// <remarks>If <paramref name="dataRow"/> is <see langword="null"/>, gets or sets the value of this column's <see cref="ScalarValue"/>.</remarks>
        /// <exception cref="ArgumentException">This column does not belong to provided <paramref name="dataRow"/>.</exception>
        /// <exception cref="InvalidOperationException">This column is read only when setting the value.</exception>
        /// <seealso cref="ScalarValue"/>
        /// <seealso cref="IsReadOnly(DataRow)"/>
        public T this[DataRow dataRow]
        {
            get
            {
                if (dataRow == null)
                    return ScalarValue;

                if (IsExpression)
                    return _expression.Eval(dataRow);

                VerifyDataRow(dataRow);
                return ValueManager[dataRow.Ordinal];
            }
            set
            {
                if (dataRow == null)
                {
                    ScalarValue = value;
                    return;
                }

                if (IsExpression)
                    throw new InvalidOperationException(Strings.Column_SetReadOnlyValue);

                VerifyDataRow(dataRow);
                var valueManager = ValueManager;
                var ordinal = dataRow.Ordinal;
                if (valueManager.IsReadOnly(ordinal))
                    throw new InvalidOperationException(Strings.Column_SetReadOnlyValue);
                valueManager[ordinal] = value;
            }
        }

        private void VerifyDataRow(DataRow dataRow)
        {
            Debug.Assert(dataRow != null);

            if (dataRow.Model != ParentModel)
                throw new ArgumentException(Strings.Column_VerifyDataRow, nameof(dataRow));
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <paramref name="dataRow"/>, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(DataRow dataRow)
        {
            if (dataRow == null)
                return false;

            VerifyDataRow(dataRow);
            return IsReadOnly(dataRow.Ordinal);
        }

        /// <summary>Gets a value indicates whether this column is readonly for provided <see cref="DataRow"/> ordinal.</summary>
        /// <param name="ordinal">The <see cref="DataRow"/> ordinal.</param>
        /// <returns><see langword="true"/> if this column is readonly for provided <see cref="DataRow"/> oridinal, otherwise <see langword="false"/>.</returns>
        public bool IsReadOnly(int ordinal)
        {
            if (IsExpression)
                return true;
            return ValueManager.IsReadOnly(ordinal);
        }

        /// <summary>Gets or sets the value of this column from provided <see cref="DataRow"/> ordinal.</summary>
        /// <param name="ordinal">The provided <see cref="DataRow"/> ordinal.</param>
        /// <returns>The value of this column from provided <see cref="DataRow"/> ordinal.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="DataRow"/> ordinal is out of range.</exception>
        /// <exception cref="InvalidOperationException">This column is readonly when setting the value.</exception>
        /// <seealso cref="IsReadOnly(int)"/>
        public T this[int ordinal]
        {
            get
            {
                var valueManager = ValueManager;
                if (ordinal < 0 || ordinal >= valueManager.RowCount)
                    throw new ArgumentOutOfRangeException(nameof(ordinal));

                return valueManager[ordinal];
            }
            set
            {
                if (IsReadOnly(ordinal))
                    throw new InvalidOperationException(Strings.Column_SetReadOnlyValue);

                var valueManager = ValueManager;
                if (ordinal < 0 || ordinal >= valueManager.RowCount)
                    throw new ArgumentOutOfRangeException(nameof(ordinal));

                if (valueManager.IsReadOnly(ordinal))
                    throw new InvalidOperationException(Strings.Column_SetReadOnlyValue);
                valueManager[ordinal] = value;
            }
        }

        internal sealed override void InsertRow(DataRow dataRow)
        {
            _valueManager.AddRow(dataRow);
        }

        internal sealed override void RemoveRow(DataRow dataRow)
        {
            _valueManager.RemoveRow(dataRow);
        }

        internal override void ClearRows()
        {
            _valueManager.ClearRows();
        }

        /// <summary>Gets or sets a scalar value for this column.</summary>
        /// <remarks>The <see cref="ScalarValue"/> can be used to temporarily store a value for this column.
        /// It does not participate value evaluaiton nor validation.</remarks>
        public T ScalarValue { get; set; }

        /// <inheritdoc/>
        public override bool IsExpression
        {
            get { return _expression != null; }
        }

        /// <summary>Evaluates the expression against the provided <see cref="DataRow"/> object.</summary>
        /// <param name="dataRow">The provided <see cref="DataRow"/> object. If <see langword="null"/> provided, it will be
        /// evaluated against the <see cref="ScalarValue"/>.</param>
        /// <returns>The result value.</returns>
        public T Eval(DataRow dataRow = null)
        {
            return IsExpression ? _expression.Eval(dataRow) : this[dataRow];
        }

        /// <inheritdoc/>
        public sealed override Type DataType
        {
            get { return typeof(T); }
        }

        private DbExpression _dbExpression;
        /// <inheritdoc/>
        public sealed override DbExpression DbExpression
        {
            get
            {
                return _dbExpression ?? (_dbExpression = _expression == null ? new DbColumnExpression(this) : _expression.GetDbExpression());
            }
        }

        /// <inheritdoc/>
        protected internal abstract bool IsNull(T value);

        /// <inheritdoc/>
        public sealed override bool IsNull(DataRow dataRow)
        {
            return IsNull(this[dataRow]);
        }

        /// <inheritdoc/>
        public sealed override IModelSet ParentModelSet
        {
            get { return _expression == null ? ParentModel : _expression.ParentModelSet; }
        }

        /// <inheritdoc/>
        public sealed override IModelSet AggregateModelSet
        {
            get { return _expression == null ? ModelSet.Empty : _expression.AggregateModelSet; }
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="value">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        protected internal abstract Column<T> CreateConst(T value);

        internal sealed override bool ShouldSerialize
        {
            get
            {
                Debug.Assert(_valueManager != null);
                return _valueManager.ShouldSerialize;
            }
        }
        
        /// <inheritdoc/>
        protected internal sealed override Column CreateParam(DataRow dataRow)
        {
            return CreateParam(this[dataRow]);
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="value">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        protected abstract Column<T> CreateParam(T value);

        internal sealed override Default CreateDefault()
        {
            return new Default<T>(this);
        }

        internal sealed override Column GetParrallel(Model model)
        {
            Debug.Assert(model.GetType() == this.ParentModel.GetType());
            return IsExpression ? _expression.GetParallelColumn(model) : model.Columns[Ordinal];
        }

        /// <summary>Defines the default constant value for this column.</summary>
        /// <param name="defaultValue">The default constant value.</param>
        /// <remarks>To define default expression value, call <see cref="ColumnExtensions.Default{T}(T, T)"/> method.</remarks>
        public void DefaultValue(T defaultValue)
        {
            AddOrUpdateInterceptor(new Default<T>(CreateConst(defaultValue)));
        }

        /// <summary>Gets the default declaration for this column.</summary>
        /// <returns>The default declaration for this column. Returns <see langword="null"/> if no default defined.</returns>
        public new Default<T> GetDefault()
        {
            return GetInterceptor<Default<T>>();
        }

        /// <inheritdoc/>
        protected internal sealed override JsonValue Serialize(int rowOrdinal)
        {
            return SerializeValue(this[rowOrdinal]);
        }

        /// <summary>Serializes value into JSON.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized JSON value.</returns>
        protected internal abstract JsonValue SerializeValue(T value);

        /// <inheritdoc/>
        protected internal sealed override void Deserialize(int ordinal, JsonValue value)
        {
            this[ordinal] = DeserializeValue(value);
        }

        /// <summary>Deserializes from JSON value.</summary>
        /// <param name="value">The JSON value.</param>
        /// <returns>The deserialized value.</returns>
        protected internal abstract T DeserializeValue(JsonValue value);

        internal virtual T GetDefaultValue(DataRow dataRow)
        {
            var defaultDef = this.GetDefault();
            return defaultDef != null ? defaultDef.DefaultValue.Eval(dataRow) : default(T);
        }

        /// <summary>Defines the computation expression for this column.</summary>
        /// <param name="computation">The computation expression.</param>
        public void ComputedAs(Column<T> computation)
        {
            Check.NotNull(computation, nameof(computation));

            Computation = computation;
        }

        /// <summary>Gets the computation expression for this column.</summary>
        public Column<T> Computation { get; private set; }

        /// <inheritdoc/>
        public sealed override Column GetComputation()
        {
            return Computation;
        }

        /// <inheritdoc/>
        public sealed override void Compute(DataRow dataRow)
        {
            if (Computation == null)
                return;

            VerifyDataRow(dataRow);
            this[dataRow] = Computation[dataRow];
        }

        /// <summary>Mapping this column with another column.</summary>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>The <see cref="ColumnMapping"/> contains <paramref name="sourceColumn"/> and this column.</returns>
        public ColumnMapping From(Column<T> sourceColumn)
        {
            Check.NotNull(sourceColumn, nameof(sourceColumn));
            return new ColumnMapping(sourceColumn, this);
        }

        /// <summary>Mapping this column with constant value.</summary>
        /// <param name="value">The constant value.</param>
        /// <returns>The <see cref="ColumnMapping"/> contains the constant value and this column.</returns>
        public ColumnMapping From(T value)
        {
            return new ColumnMapping(CreateParam(value), this);
        }

        /// <inheritdoc/>
        public sealed override ColumnMapping From(Column column)
        {
            return From((Column<T>)column);
        }
    }
}