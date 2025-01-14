﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a database table.
    /// </summary>
    /// <typeparam name="T">Entity type of database table.</typeparam>
    public sealed partial class DbTable<T> : DbSet<T>, IDbTable
        where T : Model, new()
    {
        internal static DbTable<T> Create(T modelRef, DbSession dbSession, string identifier, Action<DbTable<T>> initializer = null)
        {
            var result = new DbTable<T>(modelRef, dbSession, identifier, DataSourceKind.DbTable);
            initializer?.Invoke(result);
            result.DesignMode = false;
            return result;
        }

        internal static DbTable<T> CreateTemp(T modelRef, DbSession dbSession, string identifier, Action<DbTable<T>> initializer = null)
        {
            var result = new DbTable<T>(modelRef, dbSession, identifier, DataSourceKind.DbTempTable);
            initializer?.Invoke(result);
            result.DesignMode = false;
            return result;
        }

        internal static DbTable<T> MockTemp(T modelRef, DbSession dbSession, string identifier, Action<DbTable<T>> initializer = null)
        {
            return CreateTemp(modelRef, dbSession, identifier, initializer);
        }

        private DbTable(T model, DbSession dbSession, string identifier, DataSourceKind kind)
            : base(model, dbSession)
        {
            Debug.Assert(!string.IsNullOrEmpty(identifier));

            Identifier = identifier;
            _kind = kind;
            model.SetDataSource(this);
        }

        internal int InitialRowCount { get; set; }

        /// <summary>
        /// Gets the identifier of this table.
        /// </summary>
        public string Identifier { get; private set; }

        private string _name;
        /// <summary>
        /// Gets the name of this table.
        /// </summary>
        public string Name
        {
            get { return _name ?? Identifier; }
            internal set { _name = value; }
        }

        /// <summary>
        /// Gets the description of this table.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this DbTable object is in design mode.
        /// </summary>
        public bool DesignMode { get; private set; } = true;

        private DataSourceKind _kind;
        /// <inheritdoc />
        public override DataSourceKind Kind
        {
            get { return _kind; }
        }

        private DbSelectStatement _selectStatement;
        private DbSelectStatement SelectStatement
        {
            get { return LazyInitializer.EnsureInitialized(ref _selectStatement, () => GetSelectStatement()); }
        }

        private DbSelectStatement GetSelectStatement()
        {
            var columns = this.Model.Columns;
            var selectList = new ColumnMapping[columns.Count];
            for (int i = 0; i < selectList.Length; i++)
            {
                var column = columns[i];
                selectList[i] = new ColumnMapping(column, column);
            }
            return new DbSelectStatement(this.Model, selectList, this.FromClause, null, GetOrderBy(), -1, -1);
        }

        private IReadOnlyList<DbExpressionSort> GetOrderBy()
        {
            if (Kind == DataSourceKind.DbTable)
                return null;

            Debug.Assert(Kind == DataSourceKind.DbTempTable);
            var identity = Model.GetIdentity(true);
            if (identity == null)
                return null;
            else
                return new DbExpressionSort[] {
                    new DbExpressionSort(identity.Column.DbExpression, SortDirection.Ascending)
                };
        }

        private DbQueryStatement _queryStatement;
        internal override DbQueryStatement QueryStatement
        {
            get { return LazyInitializer.EnsureInitialized(ref _queryStatement, () => SelectStatement.RemoveSystemColumns()); }
        }

        internal override DbQueryStatement SequentialQueryStatement
        {
            get { return SelectStatement; }
        }

        private DbFromClause _fromClause;
        internal override DbFromClause FromClause
        {
            get { return LazyInitializer.EnsureInitialized(ref _fromClause, () => new DbTableClause(Model, Name)); }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates child tempoary table.
        /// </summary>
        /// <typeparam name="TChild">Child model type.</typeparam>
        /// <param name="getChildModel">Delete to get child model.</param>
        /// <returns>The child temporary table.</returns>
        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            return CreateChildAsync(null, getChildModel);
        }

        /// <summary>
        /// Creates child tempoary table.
        /// </summary>
        /// <typeparam name="TChild">Child model type.</typeparam>
        /// <param name="initializer">The child model initializer.</param>
        /// <param name="getChildModel">Delete to get child model.</param>
        /// <returns>The child temporary table.</returns>
        public Task<DbTable<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            return CreateChildAsync(initializer, getChildModel, CancellationToken.None);
        }

        /// <summary>
        /// Creates child tempoary table.
        /// </summary>
        /// <typeparam name="TChild">Child model type.</typeparam>
        /// <param name="initializer">The child model initializer.</param>
        /// <param name="getChildModel">Delete to get child model.</param>
        /// <param name="cancellationToken">The async cancellation token.</param>
        /// <returns>The child temporary table.</returns>
        public async Task<DbTable<TChild>> CreateChildAsync<TChild>(Action<TChild> initializer, Func<T, TChild> getChildModel, CancellationToken cancellationToken)
            where TChild : Model, new()
        {
            var model = VerifyCreateChild(initializer, getChildModel);

            var dbSession = DbSession;
            var name = dbSession.AssignTempTableName(model);
            var result = DbTable<TChild>.CreateTemp(model, dbSession, name);
            await dbSession.CreateTableAsync(model, true, cancellationToken);
            return result;
        }

        /// <summary>
        /// Gets the child tempoary table.
        /// </summary>
        /// <typeparam name="TChild">Child model type.</typeparam>
        /// <param name="getChildModel">Delete to get child model.</param>
        /// <returns>The child temporary table.</returns>
        public DbTable<TChild> GetChild<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            getChildModel.VerifyNotNull(nameof(getChildModel));
            var childModel = getChildModel(_);
            if (childModel == null)
                return null;
            return childModel.DataSource as DbTable<TChild>;
        }

        private sealed class ScalarParamManager
        {
            public ScalarParamManager(DataRow dataRow)
            {
                Debug.Assert(dataRow != null);
                _dataRow = dataRow;
            }

            private DataRow _dataRow;
            private Dictionary<Column, Column> _params = new Dictionary<Column, Column>();

            public Model Model
            {
                get { return _dataRow.Model; }
            }

            public Column GetParam(Column column)
            {
                Debug.Assert(column.ParentModel == _dataRow.Model);

                if (_params.ContainsKey(column))
                    return _params[column];

                var result = column.CreateParam(_dataRow);
                _params.Add(column, result);
                return result;
            }
        }

        private static IReadOnlyList<ColumnMapping> GetScalarMapping(ScalarParamManager paramManager, IReadOnlyList<ColumnMapping> mappings, bool mapToSource = false)
        {
            var result = new ColumnMapping[mappings.Count];
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                var sourceColumn = mapping.Source;
                var source = paramManager.GetParam(sourceColumn);
                result[i] = new ColumnMapping(source, mapToSource ? sourceColumn : mapping.Target);
            }
            return result;
        }

        private static DbSelectStatement GetScalarDataSource(ScalarParamManager paramManager, IReadOnlyList<ColumnMapping> parentMappings = null)
        {
            if (parentMappings == null)
                return null;

            var select = GetScalarMapping(paramManager, parentMappings, true);
            return new DbSelectStatement(paramManager.Model, select, null, null, null, -1, -1);
        }

        private DbExpression VerifyWhere(Func<T, _Boolean> where)
        {
            if (where == null)
                return null;

            var whereExpr = where(_);
            if (object.ReferenceEquals(whereExpr, null))
                return null;

            var parentModelSet = whereExpr.ScalarSourceModels;
            if (parentModelSet.Count == 0 || (parentModelSet.Count == 1 && parentModelSet.Contains(_)))
                return whereExpr.DbExpression;

            throw new ArgumentException(DiagnosticMessages.DbTable_VerifyWhere, nameof(where));
        }

        internal int UpdateOrigin(DataSource origin, int rowsAffected)
        {
            if (rowsAffected != 0)
                UpdateOriginalDataSource(origin);
            return rowsAffected;
        }

        internal bool UpdateOrigin<TSource>(DataSet<TSource> origin, bool scalarInsertSuccess)
            where TSource : Model, new()
        {
            if (scalarInsertSuccess)
                UpdateOriginalDataSource(origin == null || origin.Count != 1 ? null : origin);

            return scalarInsertSuccess;
        }

        internal void Verify<TLookup>(DbSet<TLookup> source, string paramName)
            where TLookup : Model, new()
        {
            source.VerifyNotNull(paramName);
            if (source.DbSession != DbSession)
                throw new ArgumentException(DiagnosticMessages.DbTable_InvalidDbSetSource, paramName);
        }

        internal void Verify<TLookup>(DataSet<TLookup> source, string paramName)
            where TLookup : Model, new()
        {
            source.VerifyNotNull(paramName);
        }

        internal void Verify<TSource>(DataSet<TSource> source, string sourceParamName, int rowIndex, string rowIndexParamName)
            where TSource : Model, new()
        {
            Verify(source, sourceParamName);
            if (rowIndex < 0 || rowIndex >= source.Count)
                throw new ArgumentOutOfRangeException(rowIndexParamName);
        }

        internal KeyMapping Verify<TSource>(Func<TSource, T, KeyMapping> keyMapper, string paramName, TSource source)
            where TSource : Model
        {
            keyMapper.VerifyNotNull(paramName);
            var result = keyMapper(source, _);
            if (result.IsEmpty || result.SourceModel != source || result.TargetModel != _)
                throw new ArgumentException(DiagnosticMessages.DbTable_InvalidReturnedKeyMapping, paramName);
            return result;
        }

        internal IReadOnlyList<ColumnMapping> Verify(Action<ColumnMapper, T> mapper, string paramName)
        {
            mapper.VerifyNotNull(paramName);
            var result = new ColumnMapper(null, _).Build(x => mapper(x, _));
            if (result == null || result.Count == 0)
                throw new ArgumentException(DiagnosticMessages.DbTable_EmptyColumnMapperResult, paramName);
            return result;
        }

        internal IReadOnlyList<ColumnMapping> Verify<TSource>(Action<ColumnMapper, TSource, T> mapper, string paramName, TSource source)
            where TSource : Model, new()
        {
            mapper.VerifyNotNull(paramName);
            var result = new ColumnMapper(source, _).Build(x => mapper(x, source, _));
            if (result == null || result.Count == 0)
                throw new ArgumentException(DiagnosticMessages.DbTable_EmptyColumnMapperResult, paramName);
            return result;
        }

        internal void Verify<TSource>(Action<ColumnMapper, TSource, T> mapper, string paramName)
            where TSource : Model, new()
        {
            mapper.VerifyNotNull(paramName);
        }

        private IReadOnlyList<ColumnMapping> Verify<TSource>(Action<ColumnMapper, TSource, T> mapper, TSource source)
            where TSource : Model, new()
        {
            var result = new ColumnMapper(source, _).Build(x => mapper(x, source, _));
            if (result == null || result.Count == 0)
                throw new InvalidOperationException(DiagnosticMessages.DbTable_EmptyColumnMapperResult);
            return result;
        }
    }
}
