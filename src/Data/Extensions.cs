﻿using DevZest.Data.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public static class Extensions
    {
        public static DataSet<T> GetChildDataSet<T>(this T childEntity, DataRow parentDataRow) where T : class, IEntity, new()
        {
            Verify(childEntity, nameof(childEntity));
            parentDataRow.VerifyNotNull(nameof(parentDataRow));
            return (DataSet<T>)parentDataRow[childEntity.Model];
        }

        public static DataSet<T> GetChildDataSet<T>(this T childEntity, int parentDataRowOrdinal) where T : class, IEntity, new()
        {
            var parentDataSet = Verify(childEntity, nameof(childEntity));
            return childEntity.GetChildDataSet(parentDataSet[parentDataRowOrdinal]);
        }

        private static DataSet Verify<T>(T childEntity, string paramName) where T : class, IEntity, new()
        {
            childEntity.VerifyNotNull(paramName);
            var parentDataSet = childEntity?.Model?.ParentModel?.DataSet;
            if (parentDataSet == null)
                throw new ArgumentException(DiagnosticMessages.EntityExtensions_NullParentDataSet, paramName);
            return parentDataSet;
        }

        public static KeyMapping Match<T>(this IEntity<T> source, T target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target);
        }

        public static KeyMapping Match<T>(this IEntity<T> source, IEntity<T> target)
            where T : CandidateKey
        {
            target.VerifyNotNull(nameof(target));
            return new KeyMapping(source.Model.PrimaryKey, target.Model.PrimaryKey);
        }

        public static KeyMapping Join<T>(this T sourceKey, IEntity<T> target)
            where T : CandidateKey
        {
            return new KeyMapping(sourceKey, target.Model.PrimaryKey);
        }

        public static Task<T> SingleAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, CancellationToken ct = default(CancellationToken))
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            getColumn.VerifyNotNull(nameof(getColumn));
            return dbSet.ReadSingleAsync(getColumn, false, ct);
        }

        public static Task<T> SingleOrDefaultAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, CancellationToken ct = default(CancellationToken))
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            getColumn.VerifyNotNull(nameof(getColumn));
            return dbSet.ReadSingleAsync(getColumn, true, ct);
        }

        private static async Task<T> ReadSingleAsync<TEntity, T>(this DbSet<TEntity> dbSet, Func<TEntity, T> getColumn, bool allowEmpty, CancellationToken ct)
            where TEntity : class, IEntity, new()
            where T : Column, IColumn<DbReader>, new()
        {
            var dbSession = dbSet.DbSession;
            var query = dbSession.CreateQuery<Adhoc>((builder, _) => builder.From(dbSet, out var s).Select(getColumn(s), _.AddColumn<T>()));

            using (var dbReader = await query.ExecuteDbReaderAsync(ct))
            {
                if (!(await dbReader.ReadAsync(ct)))
                {
                    if (allowEmpty)
                        return null;
                    else
                        throw new InvalidOperationException(DiagnosticMessages.Single_NoElement);
                }

                var dataSet = query.MakeDataSet();
                var result = dataSet._.GetColumn<T>();
                var dataRow = dataSet.AddRow();
                result.Read(dbReader, dataRow);
                if (await dbReader.ReadAsync(ct))
                    throw new InvalidOperationException(DiagnosticMessages.Single_MultipleElements);
                return result;
            }
        }

    }
}