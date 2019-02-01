﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    internal static class DbTableInsert<T>
        where T : class, IModelReference, new()
    {
        private static void UpdateIdentity<TSource>(DataSet<TSource> dataSet, DataRow dataRow, long? value)
            where TSource : class, IModelReference, new()
        {
            var model = dataSet._.Model;
            model.SuspendIdentity();
            dataRow.IsPrimaryKeySealed = false;
            var identityColumn = model.GetIdentity(false).Column;
            if (identityColumn is _Int32 int32Column)
                int32Column[dataRow] = (int?)value;
            else if (identityColumn is _Int64 int64Column)
                int64Column[dataRow] = value;
            else if (identityColumn is _Int16 int16Column)
                int16Column[dataRow] = (short?)value;
            else
                Debug.Fail("Identity column must be _Int32, _Int64 or _Int16.");
            model.ResumeIdentity();
            dataRow.IsPrimaryKeySealed = true;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DbSet<TSource> source, IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statement = target.BuildInsertStatement(source, columnMappings, join);
            return target.UpdateOrigin(source, await target.DbSession.InsertAsync(statement, ct));
        }

        public static async Task<int> ExecuteWithUpdateIdentityAsync<TSource>(DbTable<T> target, DbTable<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var result = await target.DbSession.InsertForIdentityAsync(source, target, columnMapper, joinTo, ct);
            return target.UpdateOrigin(source, result);
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, int rowIndex,
            IReadOnlyList<ColumnMapping> columnMappings, IReadOnlyList<ColumnMapping> join, bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            var statement = target.BuildInsertScalarStatement(source, rowIndex, columnMappings, join);
            var result = await target.DbSession.InsertScalarAsync(statement, updateIdentity, ct);
            if (updateIdentity)
                UpdateIdentity(source, source[rowIndex], result.IdentityValue);
            return target.UpdateOrigin(source, result.Success) ? 1 : 0;
        }

        public static async Task<int> ExecuteAsync<TSource>(DbTable<T> target, DataSet<TSource> source, Action<ColumnMapper, TSource, T> columnMapper, CandidateKey joinTo,
            bool updateIdentity, CancellationToken ct)
            where TSource : class, IModelReference, new()
        {
            if (source.Count == 0)
                return 0;

            var result = await target.DbSession.InsertAsync(source, target, columnMapper, joinTo, updateIdentity, ct);
            return target.UpdateOrigin(source, result);
        }
    }
}
