﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>Represents an in-memory collection of data.</summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public abstract class DataSet<T> : DataSet
        where T : Model, new()
    {
        private sealed class BaseDataSet : DataSet<T>
        {
            public BaseDataSet(T model)
                : base(model)
            {
                model.SetDataSource(this);
            }

            internal override DataSet CreateChildDataSet(DataRow parentRow)
            {
                return new ChildDataSet(this, parentRow);
            }

            public override DataRow ParentDataRow
            {
                get { return null; }
            }

            public override bool IsReadOnly
            {
                get { return Model.ParentModel != null; }
            }

            public override int IndexOf(DataRow dataRow)
            {
                return dataRow == null || dataRow.Model != Model ? -1 : dataRow.Ordinal;
            }

            internal override void CoreRemoveAt(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Ordinal == index);

                dataRow.DisposeByBaseDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }

            internal override void CoreInsert(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeByBaseDataSet(Model, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustOrdinal(i);
            }
        }

        private sealed class ChildDataSet : DataSet<T>
        {
            public ChildDataSet(BaseDataSet baseDataSet, DataRow parentRow)
                : base(baseDataSet._)
            {
                Debug.Assert(baseDataSet != null);
                _baseDataSet = baseDataSet;
                _parentRow = parentRow;
            }

            internal override DataSet CreateChildDataSet(DataRow parentRow)
            {
                throw new NotSupportedException();
            }

            private DataSet<T> _baseDataSet;
            private readonly DataRow _parentRow;
            public override DataRow ParentDataRow
            {
                get { return _parentRow; }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override int IndexOf(DataRow dataRow)
            {
                return dataRow == null || dataRow.Model != Model ? -1 : dataRow.Index;
            }

            internal override void CoreRemoveAt(int index, DataRow dataRow)
            {
                Debug.Assert(dataRow.Model == Model && dataRow.Index == index);

                dataRow.DisposeByChildDataSet();
                _rows.RemoveAt(index);
                for (int i = index; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);

                _baseDataSet.InnerRemoveAt(dataRow.Ordinal);
            }

            internal override void CoreInsert(int index, DataRow dataRow)
            {
                Debug.Assert(index >= 0 && index <= Count);
                Debug.Assert(dataRow.Model == null);

                dataRow.InitializeByChildDataSet(_parentRow, index);
                _rows.Insert(index, dataRow);
                for (int i = index + 1; i < _rows.Count; i++)
                    _rows[i].AdjustIndex(i);
                _baseDataSet.InternalInsert(GetBaseDataSetOrdinal(dataRow), dataRow);
            }

            private int GetBaseDataSetOrdinal(DataRow dataRow)
            {
                if (_baseDataSet.Count == 0)
                    return 0;

                if (Count > 1)
                {
                    if (dataRow.Index > 0)
                        return this[dataRow.Index - 1].Ordinal + 1;  // after the previous DataRow
                    else
                        return this[dataRow.Index + 1].Ordinal;  // before the next DataRow
                }

                return BinarySearchBaseDataSetOrdinal(dataRow);
            }

            private int BinarySearchBaseDataSetOrdinal(DataRow dataRow)
            {
                var parentOrdinal = dataRow.ParentDataRow.Ordinal;

                var endOrdinal = _baseDataSet.Count - 1;
                if (parentOrdinal > _baseDataSet[endOrdinal].ParentDataRow.Ordinal)
                    return endOrdinal + 1;  // after the end

                var startOrdinal = 0;
                if (parentOrdinal < _baseDataSet[startOrdinal].ParentDataRow.Ordinal)
                    return startOrdinal;  // before the start

                int resultOrdinal = endOrdinal;
                bool flagBefore = true;
                while (startOrdinal < endOrdinal)
                {
                    var mid = (startOrdinal + endOrdinal) / 2;
                    if (parentOrdinal < _baseDataSet[mid].ParentDataRow.Ordinal)
                    {
                        resultOrdinal = endOrdinal;
                        flagBefore = true;
                        endOrdinal = mid - 1;
                    }
                    else
                    {
                        resultOrdinal = startOrdinal;
                        flagBefore = false;
                        startOrdinal = mid + 1;
                    }
                }

                return flagBefore ? resultOrdinal - 1 : resultOrdinal + 1;
            }
        }

        /// <summary>
        /// Create a new DataSet.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <returns>The newly created DataSet.</returns>
        public static DataSet<T> Create(Action<T> initializer = null)
        {
            var model = new T();
            model.Initialize(initializer);
            return Create(model);
        }

        internal static DataSet<T> Create(T modelRef)
        {
            return new BaseDataSet(modelRef);
        }

        /// <summary>
        /// Clones this DataSet.
        /// </summary>
        /// <returns>The cloned new DataSet.</returns>
        public new DataSet<T> Clone()
        {
            var modelRef = _.MakeCopy(false);
            return Create(modelRef);
        }

        internal sealed override DataSet InternalClone()
        {
            return this.Clone();
        }

        private DataSet(T model)
        {
            Debug.Assert(model != null);
            _ = model;
        }

        /// <summary>
        /// Gets the model associated with this DataSet.
        /// </summary>
        public T _ { get; private set; }

        /// <inheritdoc />
        public sealed override Model Model
        {
            get { return _; }
        }

        /// <summary>
        /// Deserializes from JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized DataSet.</returns>
        public static DataSet<T> ParseJson(string json)
        {
            return ParseJson(null, json);
        }

        /// <summary>
        /// Deserializes from JSON string.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        /// <param name="json">The JSON string.</param>
        /// <param name="customizer">The customizer.</param>
        /// <returns></returns>
        public static DataSet<T> ParseJson(Action<T> initializer, string json, IJsonCustomizer customizer = null)
        {
            json.VerifyNotEmpty(nameof(json));

            return (DataSet<T>)(JsonReader.Create(json, customizer).Parse(() => Create(initializer), true));
        }

        private static DbQuery<TChild> GetChildQuery<TChild>(DbSet<TChild> dbSet, DataRow parentRow, IReadOnlyList<ColumnMapping> parentRelationship, Action<TChild> initializer)
            where TChild : Model, new()
        {
            var dbSession = dbSet.DbSession;
            var childModel = dbSet._.MakeCopy(false);
            childModel.Initialize(initializer);
            var queryStatement = dbSet.QueryStatement.BuildQueryStatement(childModel, builder => builder.Where(parentRow, parentRelationship), null);
            return dbSession.PerformCreateQuery(childModel, queryStatement);
        }

        /// <summary>
        /// Fills child DataSet from source DbSet.
        /// </summary>
        /// <typeparam name="TChild">Model type of child DataSet.</typeparam>
        /// <param name="dataRowOrdinal">The ordinal of parent <see cref="DataRow"/>.</param>
        /// <param name="getChildModel">The delegate to return child model.</param>
        /// <param name="sourceData">The source Data.</param>
        /// <param name="initializer">The initializer.</param>
        /// <param name="cancellationToken">Async operation cancellation token.</param>
        /// <returns>The child DataSet.</returns>
        public async Task<DataSet<TChild>> FillChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var dataRow = this[dataRowOrdinal];
            var childModel = _.Verify(getChildModel, nameof(getChildModel));

            var childDataSet = (DataSet<TChild>)dataRow[childModel];
            var mappings = childModel.ParentRelationship;
            var childQuery = GetChildQuery(sourceData, dataRow, mappings, initializer);
            await sourceData.DbSession.FillDataSetAsync(childQuery, childDataSet, cancellationToken);

            return childDataSet;
        }

        /// <summary>
        /// Fills child DataSet from source DbSet.
        /// </summary>
        /// <typeparam name="TChild">Model type of child DataSet.</typeparam>
        /// <param name="dataRowOrdinal">The ordinal of parent <see cref="DataRow"/>.</param>
        /// <param name="getChildModel">The delegate to return child model.</param>
        /// <param name="sourceData">The source Data.</param>
        /// <param name="initializer">The initializer.</param>
        /// <returns>The child DataSet.</returns>
        public Task<DataSet<TChild>> FillChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, Action<TChild> initializer = null)
            where TChild : Model, new()
        {
            return FillChildAsync(dataRowOrdinal, getChildModel, sourceData, initializer, CancellationToken.None);
        }

        /// <summary>
        /// Fills child DataSet from source DbSet.
        /// </summary>
        /// <typeparam name="TChild">Model type of child DataSet.</typeparam>
        /// <param name="dataRowOrdinal">The ordinal of parent <see cref="DataRow"/>.</param>
        /// <param name="getChildModel">The delegate to return child model.</param>
        /// <param name="sourceData">The source Data.</param>
        /// <param name="cancellationToken">Async operation cancellation token.</param>
        /// <returns>The child DataSet.</returns>
        public Task<DataSet<TChild>> FillChildAsync<TChild>(int dataRowOrdinal, Func<T, TChild> getChildModel, DbSet<TChild> sourceData, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            return FillChildAsync(dataRowOrdinal, getChildModel, sourceData, null, cancellationToken);
        }

        /// <summary>
        /// Gets the child DataSet.
        /// </summary>
        /// <typeparam name="TChild">Model type of child DataSet.</typeparam>
        /// <param name="getChildModel">The delegate to return child model.</param>
        /// <param name="ordinal">The ordinal of parent DataRow.</param>
        /// <returns>The child DataSet.</returns>
        public DataSet<TChild> GetChild<TChild>(Func<T, TChild> getChildModel, int ordinal)
            where TChild : Model, new()
        {
            return GetChild(getChildModel, this[ordinal]);
        }

        /// <summary>
        /// Gets the child DataSet.
        /// </summary>
        /// <typeparam name="TChild">Model type of child DataSet.</typeparam>
        /// <param name="getChildModel">The delegate to return child model.</param>
        /// <param name="dataRow">The parent DataRow.</param>
        /// <returns>The child DataSet.</returns>
        public DataSet<TChild> GetChild<TChild>(Func<T, TChild> getChildModel, DataRow dataRow = null)
            where TChild : Model, new()
        {
            var childModel = _.Verify(getChildModel, nameof(getChildModel));
            return dataRow == null ? childModel.DataSet as DataSet<TChild> : childModel.GetChildDataSet(dataRow);
        }

        private Action<DataRow> GetUpdateAction(Action<T, DataRow> updateAction)
        {
            Action<DataRow> result;
            if (updateAction == null)
                result = null;
            else
                result = (DataRow x) => updateAction(_, x);
            return result;
        }

        /// <summary>
        /// Inserts DataRow at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <param name="updateAction">The delegate to initialize the DataRow.</param>
        /// <returns>The inserted DataRow.</returns>
        public DataRow Insert(int index, Action<T, DataRow> updateAction)
        {
            var result = new DataRow();
            Insert(index, result, GetUpdateAction(updateAction));
            return result;
        }

        /// <summary>
        /// Adds DataRow into this DataSet.
        /// </summary>
        /// <param name="updateAction">The delegate to initialize the DataRow.</param>
        /// <returns></returns>
        public DataRow AddRow(Action<T, DataRow> updateAction = null)
        {
            return Insert(Count, updateAction);
        }

        /// <summary>
        /// Ensures this DataSet is initialized.
        /// </summary>
        /// <returns>This DataSet for fluent coding.</returns>
        /// <remarks>Normally DataSet will be initialized implicitly when first DataRow is added.
        /// You can call this method explicitly when necessary.</remarks>
        public DataSet<T> EnsureInitialized()
        {
            Model.EnsureInitialized();
            return this;
        }

        /// <summary>
        /// Filters this DataSet for JSON serialization.
        /// </summary>
        /// <param name="filter">The JSON filter.</param>
        /// <returns>The JSON view.</returns>
        public JsonView<T> Filter(JsonFilter filter)
        {
            filter.VerifyNotNull(nameof(filter));
            return new JsonView<T>(_, filter);
        }

        /// <summary>
        /// Filters this DataSet for JSON serialization.
        /// </summary>
        /// <param name="filters">The JSON filters.</param>
        /// <returns>The JSON view.</returns>
        public JsonView<T> Filter(params JsonFilter[] filters)
        {
            return new JsonView<T>(_, JsonFilter.Join(filters));
        }

        /// <summary>
        /// Gets the model of this DataSet.
        /// </summary>
        public T Entity
        {
            get { return _; }
        }
    }
}
