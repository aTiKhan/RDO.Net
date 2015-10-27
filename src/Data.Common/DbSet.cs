﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class DbSet<T> : DataSource, IDbSet
        where T : Model, new()
    {
        internal DbSet(T model, DbSession dbSession)
            : base(model)
        {
            Debug.Assert(dbSession != null);
            DbSession = dbSession;
            _ = model;
        }

        public DbSession DbSession { get; private set; }

        public T _ { get; private set; }

        public abstract DbQueryStatement QueryStatement { get; }

        public abstract DbQueryStatement SequentialQueryStatement { get; }

        public abstract DbFromClause FromClause { get; }

        public DbQuery<T> Where(Func<T, _Boolean> predicate)
        {
            Check.NotNull(predicate, nameof(predicate));

            T newModel;
            var queryBuilder = GetSimpleQueryBuilder(out newModel);
            queryBuilder.Where(predicate(_));
            return queryBuilder.ToQuery(newModel);
        }

        public DbQuery<T> OrderBy(params Func<T, ColumnSort>[] fnOrderByList)
        {
            return OrderBy(-1, -1, fnOrderByList);
        }

        public DbQuery<T> OrderBy(int offset, int fetch, params Func<T, ColumnSort>[] fnOrderByList)
        {
            Check.NotNull(fnOrderByList, nameof(fnOrderByList));

            T newModel;
            var queryBuilder = GetSimpleQueryBuilder(out newModel);
            var orderByList = new ColumnSort[fnOrderByList.Length];
            for (int i = 0; i < fnOrderByList.Length; i++)
                orderByList[i] = fnOrderByList[i](_);
            queryBuilder.OrderBy(offset, fetch, orderByList);
            return queryBuilder.ToQuery(newModel);
        }

        internal DbQueryBuilder GetSimpleQueryBuilder()
        {
            T newModel;
            return GetSimpleQueryBuilder(out newModel);
        }

        private DbQueryBuilder GetSimpleQueryBuilder(out T newModel)
        {
            var oldModel = _;
            newModel = Data.Model.Clone(oldModel, false);
            return DbQueryBuilder.SelectAll(DbSession, newModel, oldModel);
        }

        internal TChild VerifyCreateChild<TChild>(Func<T, TChild> getChildModel)
            where TChild : Model, new()
        {
            if (Kind == DataSourceKind.DbTable)
                throw new InvalidOperationException(Strings.DbSet_VerifyCreateChild_InvalidDataSourceKind);
            Check.NotNull(getChildModel, nameof(getChildModel));

            _.EnsureChildModelsInitialized();
            var childModel = getChildModel(_);
            if (childModel == null || childModel.ParentModel != _)
                throw new ArgumentException(Strings.InvalidChildModelGetter, nameof(getChildModel));
            if (childModel.DataSource != null)
                throw new InvalidOperationException(Strings.DbSet_VerifyCreateChild_AlreadyCreated);

            return childModel;
        }

        public DbTable<T> ToTempTable(Action<T> initializer = null)
        {
            T model = Data.Model.Clone(_, false);
            if (initializer != null)
                initializer(model);
            return QueryStatement.MakeQueryBuilder(DbSession, model, false).ToTempTable(model);
        }

        public Task<DbTable<T>> ToTempTableAsync(Action<T> initializer = null)
        {
            return ToTempTableAsync(initializer, CancellationToken.None);
        }

        public async Task<DbTable<T>> ToTempTableAsync(Action<T> initializer, CancellationToken cancellationToken)
        {
            T model = Data.Model.Clone(_, false);
            if (initializer != null)
                initializer(model);
            return await QueryStatement.MakeQueryBuilder(DbSession, model, false).ToTempTableAsync(model, cancellationToken);
        }

        public abstract int GetInitialRowCount();

        public Task<int> GetInitialRowCountAsync()
        {
            return GetInitialRowCountAsync(CancellationToken.None);
        }

        public abstract Task<int> GetInitialRowCountAsync(CancellationToken cancellationToken);

        public DataSet<T> ToDataSet()
        {
            T model = Data.Model.Clone(this._, false);
            var result = DataSet<T>.Create(model);
            DbSession.RecursiveFillDataSet(this, result);
            return result;
        }

        public async Task<DataSet<T>> ToDataSetAsync(CancellationToken cancellationToken)
        {
            T model = Data.Model.Clone(this._, false);
            var result = DataSet<T>.Create(model);
            await DbSession.RecursiveFillDataSetAsync(this, result, cancellationToken);
            return result;
        }

        public Task<DataSet<T>> ToDataSetAsync()
        {
            return ToDataSetAsync(CancellationToken.None);
        }

        public string ToJsonString(bool isPretty)
        {
            return ToDataSet().ToJsonString(isPretty);
        }

        public Task<string> ToJsonStringAsync(bool isPretty)
        {
            return ToJsonStringAsync(isPretty, CancellationToken.None);
        }

        public async Task<string> ToJsonStringAsync(bool isPretty, CancellationToken cancellationToken)
        {
            return (await ToDataSetAsync(cancellationToken)).ToJsonString(isPretty);
        }

        public DbQuery<T> Union(DbSet<T> dbSet)
        {
            return Union(dbSet, DbUnionKind.Union);
        }

        public DbQuery<T> UnionAll(DbSet<T> dbSet)
        {
            return Union(dbSet, DbUnionKind.UnionAll);
        }

        private DbQuery<T> Union(DbSet<T> dbSet, DbUnionKind kind)
        {
            Check.NotNull(dbSet, nameof(dbSet));

            var model = Data.Model.Clone(_, false);
            var queryStatement1 = this.GetSimpleQueryBuilder().BuildQueryStatement(false);
            var queryStatement2 = dbSet.GetSimpleQueryBuilder().BuildQueryStatement(false);
            return new DbQuery<T>(model, DbSession, new DbUnionStatement(model, queryStatement1, queryStatement2, kind));
        }

        public int Count()
        {
            var query = BuildCountQuery();
            using (var reader = DbSession.ExecuteDbReader(query))
            {
                int? result = null;
                if (reader.Read())
                    result = ((_Int32)query._.Columns[0])[reader];
                return result.HasValue ? result.GetValueOrDefault() : 0;
            }
        }

        private DbQuery<Adhoc> BuildCountQuery()
        {
            return DbSession.CreateQuery((DbAggregateQueryBuilder builder, Adhoc adhoc) =>
            {
                T m;
                builder.From(this, out m)
                    .Select(this._.Columns[0].CountRows(), adhoc, "Result");
            });
        }

        public Task<int> CountAsync()
        {
            return CountAsync(CancellationToken.None);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var query = BuildCountQuery();
            using (var reader = await DbSession.ExecuteDbReaderAsync(query, cancellationToken))
            {
                int? result = null;
                if (await reader.ReadAsync(cancellationToken))
                    result = ((_Int32)query._.Columns[0])[reader];
                return result.HasValue ? result.GetValueOrDefault() : 0;
            }
        }

        private DbFromClause Join(IDbTable dbTable, IList<ColumnMapping> keyMappings)
        {
            return new DbJoinClause(DbJoinKind.InnerJoin, FromClause, dbTable.FromClause, new ReadOnlyCollection<ColumnMapping>(keyMappings));
        }

        internal abstract Column GetSourceColumn(int ordinal);
    }
}