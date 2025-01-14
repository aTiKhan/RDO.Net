﻿using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data
{
    partial class DbQueryBuilder
    {
        internal virtual DbQueryStatement BuildQueryStatement(DbSelectStatement query, Action<DbQueryBuilder> action, DbTable<SequentialKey> sequentialKeys)
        {

            Initialize(query);

            var sourceModel = query.Model;
            Debug.Assert(Model.GetType() == sourceModel.GetType());

            Column sysParentRowId = sourceModel.GetSysParentRowIdColumn(createIfNotExist: false);
            if (sysParentRowId != null && sysParentRowId.Ordinal < query.Select.Count)
                Model.GetSysParentRowIdColumn(createIfNotExist: true);

            Column sysRowId = sourceModel.GetSysRowIdColumn(createIfNotExist: false);
            if (sysRowId != null && sysRowId.Ordinal < query.Select.Count)
                Model.GetSysRowIdColumn(createIfNotExist: true);

            var columns = Model.AllColumns;
            var sourceColumns = sourceModel.AllColumns;

            Debug.Assert(columns.Count <= sourceColumns.Count);
            Debug.Assert(columns.Count <= query.Select.Count);
            for (int i = 0; i < columns.Count; i++)
                SelectList.Add(new ColumnMapping(query.Select[i].SourceExpression, columns[i]));

            action?.Invoke(this);

            return BuildQueryStatement(sequentialKeys);
        }

        internal DbQueryStatement BuildQueryStatement(Model sourceModel, Action<DbQueryBuilder> action, DbTable<SequentialKey> sequentialKeys)
        {
            From(sourceModel);
            var sourceColumns = sourceModel.Columns;
            var targetColumns = Model.Columns;
            Debug.Assert(targetColumns.Count <= sourceColumns.Count);
            for (int i = 0; i < targetColumns.Count; i++)
            {
                var targetColumn = targetColumns[i];
                SelectCore(sourceColumns.AutoSelect(targetColumn), targetColumn);
            }

            action?.Invoke(this);

            return BuildQueryStatement(sequentialKeys);
        }

        private static Column GetColumnByOriginalId(IReadOnlyDictionary<ColumnId, IColumns> columnsByOriginalId, ColumnId originalId)
        {
            if (columnsByOriginalId.TryGetValue(originalId, out var columns))
            {
                if (columns.Count == 1)
                    return columns.Single();
            }
            return null;
        }

        internal DbQueryStatement BuildQueryStatement(DbTable<SequentialKey> sequentialKeys)
        {
            var parentRowIdIdentity = SelectSysParentRowId();
            var rowIdIdentity = SelectSysRowId(sequentialKeys);

            var select = NormalizeSelectList();
            if (parentRowIdIdentity == null && rowIdIdentity == null)
            {
                var result = EliminateUnionSubQuery(select);
                if (result != null)
                    return result;
            }
            var orderBy = GetOrderBy(parentRowIdIdentity, rowIdIdentity);
            return BuildSelectStatement(select, FromClause, WhereExpression, orderBy);
        }

        private IReadOnlyList<DbExpressionSort> GetOrderBy(Identity parentRowIdIdentity, Identity rowIdIdentity)
        {
            if (rowIdIdentity != null)
            {
                return new DbExpressionSort[]
                {
                    GetDbExpressionSort(rowIdIdentity)
                };
            }

            if (parentRowIdIdentity != null)
                return GetOrderBy(parentRowIdIdentity);

            return OrderByList;
        }

        private static DbExpressionSort GetDbExpressionSort(Identity identity)
        {
            return new DbExpressionSort(identity.Column.DbExpression, identity.Increment > 0 ? SortDirection.Ascending : SortDirection.Descending);
        }

        private IReadOnlyList<DbExpressionSort> GetOrderBy(Identity parentIdentity)
        {
            Debug.Assert(parentIdentity != null);
            var orderByListCount = OrderByList == null ? 0 : OrderByList.Count;
            var result = new DbExpressionSort[orderByListCount + 1];
            result[0] = GetDbExpressionSort(parentIdentity);
            for (int i = 0; i < orderByListCount; i++)
                result[i + 1] = OrderByList[i];

            return result;
        }

        private Identity SelectSysParentRowId()
        {
            var parentSequentialKeyModel = Model.ParentSequentialKeyModel;
            if (parentSequentialKeyModel == null)
                return null;

            var sysParentRowId = Model.GetSysParentRowIdColumn(createIfNotExist: true);
            var relationship = ResolveRelationship(Model.ParentRelationship, parentSequentialKeyModel);
            Join(parentSequentialKeyModel, DbJoinKind.InnerJoin, relationship);
            var result = parentSequentialKeyModel.GetIdentity(true);
            SelectCore(result.Column, sysParentRowId);
            return result;
        }

        private Identity SelectSysRowId(DbTable<SequentialKey> sequentialKeys)
        {
            if (sequentialKeys == null)
                return null;
            var sequentialKeyModel = sequentialKeys.Model;

            var sysRowId = Model.GetSysRowIdColumn(createIfNotExist: true);
            var relationship = ResolveRelationship(Model.PrimaryKey.UnsafeJoin((CandidateKey)sequentialKeyModel.PrimaryKey), (Model)sequentialKeyModel);
            Join(sequentialKeyModel, DbJoinKind.InnerJoin, relationship);
            var result = sequentialKeyModel.GetIdentity(true);
            Debug.Assert(!ReferenceEquals(result.Int32Column, null));
            Select(result.Int32Column, sysRowId);
            return result;
        }

        private IReadOnlyList<ColumnMapping> ResolveRelationship(IReadOnlyList<ColumnMapping> relationship, Model targetModel)
        {
            var result = new ColumnMapping[relationship.Count];
            for (int i = 0; i < relationship.Count; i++)
            {
                var mapping = relationship[i];
                var source = GetSource(mapping.Source.Ordinal);
                var targetColumn = GetCorrespondingPrimaryKeyColumn(mapping.Target, targetModel);
                result[i] = new ColumnMapping(source, targetColumn);
            }
            return result;
        }

        private DbExpression GetSource(int ordinal)
        {
            foreach (var select in SelectList)
            {
                if (select.Target.Ordinal == ordinal)
                    return select.SourceExpression;
            }

            return DbConstantExpression.Null;
        }

        private static Column GetCorrespondingPrimaryKeyColumn(Column column, Model targetModel)
        {
            Debug.Assert(column != null);
            Debug.Assert(targetModel != null);

            if (column.ParentModel == targetModel)
                return column;

            var primaryKey = column.ParentModel.PrimaryKey;
            for (int i = 0; i < primaryKey.Count; i++)
            {
                if (column == primaryKey[i].Column)
                    return targetModel.PrimaryKey[i].Column;
            }

            Debug.Fail("Cannot match primary key column.");
            return null;
        }

        private IReadOnlyList<ColumnMapping> NormalizeSelectList()
        {
            var result = new ColumnMapping[Model.TotalColumnCount];

            var allColumns = Model.AllColumns;

            foreach (var selectItem in SelectList)
                result[selectItem.Target.Ordinal] = selectItem;

            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].Target == null)
                    result[i] = new ColumnMapping(DbConstantExpression.Null, Model.AllColumns[i]);
            }

            return result;
        }

        internal void Where(DataRow parentRow, IReadOnlyList<ColumnMapping> parentRelationship)
        {
            var primaryKey = parentRow.Model.PrimaryKey;
            Debug.Assert(primaryKey != null);
            foreach (var columnSort in primaryKey)
            {
                var column = columnSort.Column;
                var param = column.CreateParam(parentRow).DbExpression;
                var sourceColumnOrdinal = parentRelationship.Where(x => x.Target.Ordinal == column.Ordinal).Single().Source.Ordinal;
                var sourceExpression = SelectList[sourceColumnOrdinal].SourceExpression;
                var equalCondition = new DbBinaryExpression(typeof(bool?), BinaryExpressionKind.Equal, sourceExpression, param);
                WhereExpression = WhereExpression == null ? equalCondition : new DbBinaryExpression(typeof(bool?), BinaryExpressionKind.And, equalCondition, WhereExpression);
            }
        }
    }
}
