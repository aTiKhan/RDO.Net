﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DevZest.Data.SqlServer
{
    internal sealed class SqlGenerator : DbFromClauseVisitor
    {
        internal static SqlGenerator Select(SqlSession sqlSession, DbQueryStatement query)
        {
            return sqlSession._sqlGeneratorCache.GetValue(query, (DbQueryStatement x) =>
            {
                var result = new SqlGenerator(sqlSession.SqlVersion);
                x.Accept(result);
                return result;
            });
        }

        internal static SqlGenerator Insert(SqlSession sqlSession, DbSelectStatement statement, IDbTable identityOutput)
        {
            var result = new SqlGenerator(sqlSession.SqlVersion);
            var model = statement.Model;
            var sqlBuilder = result.SqlBuilder;

            if (model.IsIdentitySuspended())
                SetIdentityInsert(sqlBuilder, model, true);

            BuildInsertIntoClause(sqlBuilder, statement.Model, statement.Select);

            if (identityOutput != null)
            {
                var identityColumn = model.GetIdentity(false).Column;
                var identityOutputNewValue = ((IIdentityOutput)identityOutput.Model).NewValue;
                sqlBuilder.AppendLine(string.Format("OUTPUT INSERTED.{0} INTO {1} ({2})",
                    identityColumn.DbColumnName.ToQuotedIdentifier(),
                    identityOutput.Name.ToQuotedIdentifier(),
                    identityOutputNewValue.DbColumnName.ToQuotedIdentifier()));
            }

            if (statement.Select == null)
                statement.From.Accept(result);
            else if (statement.Select.Count > 0)
                statement.Accept(result);

            if (model.IsIdentitySuspended())
                SetIdentityInsert(sqlBuilder, model, false);

            return result;
        }

        private static void SetIdentityInsert(IndentedStringBuilder sqlBuilder, Model model, bool on)
        {
            var dbTableName = model.GetDbTableName();
            var isTempTable = dbTableName.StartsWith("#");
            if (model.GetIdentity(isTempTable) != null)
                sqlBuilder.AppendLine(string.Format("SET IDENTITY_INSERT {0} {1};", model.GetDbTableClause().Name.ToQuotedIdentifier(), on ? "ON" : "OFF"));
        }

        private static void BuildInsertIntoClause(IndentedStringBuilder sqlBuilder, Model model, IReadOnlyList<ColumnMapping> select)
        {
            var insertList = GetInsertList(model, select);

            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append(model.GetDbTableClause().Name.ToQuotedIdentifier());

            if (insertList.Count == 0)
            {
                sqlBuilder.AppendLine(" DEFAULT VALUES;");
                return;
            }
            else
                sqlBuilder.AppendLine();

            sqlBuilder.Append('(');
            for (int i = 0; i < insertList.Count; i++)
            {
                sqlBuilder.Append(insertList[i].DbColumnName.ToQuotedIdentifier());
                if (i != insertList.Count - 1)
                    sqlBuilder.Append(", ");
            }
            sqlBuilder.AppendLine(")");
        }

        private static IList<Column> GetInsertList(Model model, IReadOnlyList<ColumnMapping> select)
        {
            if (select != null)
                return select.Select(x => x.Target).ToList();
            else
                return model.GetInsertableColumns().ToList();
        }

        internal static SqlGenerator InsertScalar(SqlSession sqlSession, DbSelectStatement statement, bool outputIdentity, out SqlParameter scopeIdentityParam)
        {
            var result = new SqlGenerator(sqlSession.SqlVersion);
            var model = statement.Model;
            var sqlBuilder = result.SqlBuilder;

            if (model.IsIdentitySuspended())
                SetIdentityInsert(sqlBuilder, model, true);

            BuildInsertIntoClause(sqlBuilder, model, statement.Select);

            if (statement.Select.Count > 0)
                statement.Accept(result);

            if (model.IsIdentitySuspended())
                SetIdentityInsert(sqlBuilder, model, false);

            if (outputIdentity)
            {
                var sqlType = SqlType.BigInt(new _Int64());
                scopeIdentityParam = sqlType.CreateSqlParameter("@scopeIdentity", ParameterDirection.Output, null, sqlSession.SqlVersion);

                sqlBuilder.AppendLine();
                sqlBuilder.Append("SET @scopeIdentity = CAST(SCOPE_IDENTITY() AS BIGINT);");
            }
            else
                scopeIdentityParam = null;

            return result;
        }

        internal static SqlGenerator Update(SqlSession sqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(sqlSession.SqlVersion);
            var sqlBuilder = result.SqlBuilder;
            var model = statement.Model;
            var selectList = statement.Select;
            var from = statement.From ?? model.GetDbTableClause();
            var where = statement.Where;

            result.ModelAliasManager = SqlServer.ModelAliasManager.Create(from);

            sqlBuilder.Append("UPDATE ");
            sqlBuilder.Append(result.ModelAliasManager[model].ToQuotedIdentifier());
            sqlBuilder.AppendLine(" SET");
            sqlBuilder.IndentLevel++;
            for (int i = 0; i < selectList.Count; i++)
            {
                var select = selectList[i];
                sqlBuilder.Append(select.Target.DbColumnName.ToQuotedIdentifier());
                sqlBuilder.Append(" = ");
                select.SourceExpression.Accept(result._expressionGenerator);
                if (i != selectList.Count - 1)
                    sqlBuilder.Append(',');
                sqlBuilder.AppendLine();
            }
            sqlBuilder.IndentLevel--;

            result.VisitingQueryStatement();
            result.GenerateFromClause(from);
            if (where != null)
                result.GenerateWhereClause(where);
            result.VisitedQueryStatement(statement);

            return result;
        }

        internal static SqlGenerator Delete(SqlSession sqlSession, DbSelectStatement statement)
        {
            var result = new SqlGenerator(sqlSession.SqlVersion);
            var sqlBuilder = result.SqlBuilder;
            var model = statement.Model;
            var selectList = statement.Select;
            var from = statement.From ?? model.GetDbTableClause();
            var where = statement.Where;

            result.ModelAliasManager = SqlServer.ModelAliasManager.Create(from);

            sqlBuilder.Append("DELETE ");
            sqlBuilder.AppendLine(result.ModelAliasManager[model].ToQuotedIdentifier());

            result.VisitingQueryStatement();
            result.GenerateFromClause(from);
            if (where != null)
                result.GenerateWhereClause(where);
            result.VisitedQueryStatement(statement);

            return result;
        }

        private SqlGenerator(SqlVersion sqlVersion)
        {
            SqlVersion = sqlVersion;
            SqlBuilder = new IndentedStringBuilder();
            _expressionGenerator = new ExpressionGenerator()
            {
                SqlVersion = this.SqlVersion,
                SqlBuilder = this.SqlBuilder,
            };
        }

        private readonly SqlVersion SqlVersion;

        private readonly IndentedStringBuilder SqlBuilder = new IndentedStringBuilder();

        public override void Visit(DbTableClause table)
        {
            if (TryGenerateJsonRowSet(table))
                return;

            var model = table.Model;
            if (model is SqlXmlNode xmlModel)
            {
                xmlModel.SourceData.Accept(_expressionGenerator);
                SqlBuilder.Append(".nodes(").AppendSingleQuoted(xmlModel.XPath).Append(')');
                var alias = ModelAliasManager[model].ToQuotedIdentifier();
                SqlBuilder.Append(' ').Append(alias).Append('(').Append(nameof(SqlXmlNode.Xml).ToQuotedIdentifier()).Append(')');
            }
            else
            {
                var tableName = table.Name.ToQuotedIdentifier();
                var alias = ModelAliasManager[model].ToQuotedIdentifier();
                SqlBuilder.Append(tableName);
                if (alias != tableName)
                    SqlBuilder.Append(' ').Append(alias);
            }
        }

        private bool TryGenerateJsonRowSet(DbTableClause table)
        {
            var model = table.Model;
            var jsonParam = model.GetSourceJsonParam();
            if (jsonParam is null)
                return false;

            SqlBuilder.Append("OPENJSON(");
            jsonParam.DbExpression.Accept(_expressionGenerator);
            SqlBuilder.AppendLine(") WITH (");
            SqlBuilder.IndentLevel++;
            var columns = model.GetColumns();
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var columnName = column.DbColumnName.ToQuotedIdentifier();
                SqlBuilder.Append(columnName).Append(' ').Append(column.GetSqlType().GetDataTypeSql(SqlVersion));
                if (i < columns.Count - 1)
                    SqlBuilder.Append(',').AppendLine();
            }
            var alias = ModelAliasManager[model].ToQuotedIdentifier();
            SqlBuilder.Append(") AS ").Append(alias);
            SqlBuilder.IndentLevel--;
            return true;
        }

        public override void Visit(DbJoinClause join)
        {
            SqlBuilder.Append("(");
            join.Left.Accept(this);
            SqlBuilder.AppendLine();

            SqlBuilder.AppendLine(join.Kind.ToSql());

            join.Right.Accept(this);

            GenerateJoinOnClause(join.On);

            SqlBuilder.Append(")");
        }

        private void GenerateJoinOnClause(IReadOnlyList<ColumnMapping> joinOn)
        {
            if (joinOn == null)
                return;

            SqlBuilder.AppendLine();
            SqlBuilder.Append("ON ");
            for (int i = 0; i < joinOn.Count; i++)
            {
                if (i > 0)
                    SqlBuilder.Append(" AND ");
                var columnMapping = joinOn[i];
                var source = columnMapping.SourceExpression;
                var target = columnMapping.Target;
                columnMapping.SourceExpression.Accept(_expressionGenerator);
                SqlBuilder.Append(" = ");
                columnMapping.TargetExpression.Accept(_expressionGenerator);
            }
        }

        private IModelAliasManager ModelAliasManager
        {
            get { return _expressionGenerator.ModelAliasManager; }
            set { _expressionGenerator.ModelAliasManager = value; }
        }

        private readonly ExpressionGenerator _expressionGenerator;

        private int _queryStatementCount = 0;

        private bool IsTopLevelQuery
        {
            get { return _queryStatementCount == 1; }
        }

        public override void Visit(DbSelectStatement selectStatement)
        {
            VisitingQueryStatement();

            if (selectStatement.From == null)
                GenerateSelectClause(selectStatement.Select, appendLine: false);
            else
            {
                var savedModelAliasManager = ModelAliasManager;
                ModelAliasManager = SqlServer.ModelAliasManager.Create(selectStatement.From);

                GenerateSelectClause(selectStatement.Select, appendLine: true);
                GenerateFromClause(selectStatement.From);
                GenerateWhereClause(selectStatement.Where);
                GenerateGroupByClause(selectStatement.GroupBy);
                GenerateHavingClause(selectStatement.Having);
                if (IsTopLevelQuery || selectStatement.Offset != -1 || selectStatement.Fetch != -1)
                    GenerateOrderByClause(selectStatement.OrderBy, selectStatement.Offset, selectStatement.Fetch);

                ModelAliasManager = savedModelAliasManager;
            }
            VisitedQueryStatement(selectStatement);
        }

        public override void Visit(DbUnionStatement unionStatement)
        {
            VisitingQueryStatement();
            _countUnionStatement++;

            unionStatement.Query1.Accept(this);
            SqlBuilder.AppendLine();
            SqlBuilder.AppendLine(unionStatement.Kind == DbUnionKind.Union ? "UNION" : "UNION ALL");
            unionStatement.Query2.Accept(this);

            _countUnionStatement--;
            VisitedQueryStatement(unionStatement);
        }

        private void VisitingQueryStatement()
        {
            _queryStatementCount++;

            if (!IsTopLevelQuery)
                SqlBuilder.Append("(");
        }

        private int _countUnionStatement;
        private bool IsUnionStatement
        {
            get { return _countUnionStatement > 0; }
        }

        private void VisitedQueryStatement(DbQueryStatement query)
        {
            if (IsTopLevelQuery)
                SqlBuilder.AppendLine(";");
            else
            {
                if (IsUnionStatement)
                    SqlBuilder.Append(")");
                else
                {
                    var alias = ModelAliasManager[query.Model].ToQuotedIdentifier();
                    SqlBuilder.Append(") ").Append(alias);
                }
            }

            _queryStatementCount--;
        }

        private void GenerateSelectClause(IReadOnlyList<ColumnMapping> select, bool appendLine)
        {
            SqlBuilder.Append("SELECT");
            GenerateExpressionList(select.Count, i => select[i].SourceExpression, i => select[i].Target, appendLine);
        }

        private void GenerateExpressionList(int count, Func<int, DbExpression> getSelectExpression, Func<int, Column> getTargetColumn, bool appendLine)
        {
            Debug.Assert(getSelectExpression != null);

            SqlBuilder.IndentLevel++;

            if (count == 1)
                SqlBuilder.Append(" ");
            else
                SqlBuilder.AppendLine();

            for (int i = 0; i < count; i++)
            {
                var selectExpression = getSelectExpression(i);
                selectExpression.Accept(_expressionGenerator);
                if (getTargetColumn != null)
                {
                    var targetColumn = getTargetColumn(i);
                    if (targetColumn != null)
                        SqlBuilder.Append(" AS ").Append(targetColumn.DbColumnName.ToQuotedIdentifier());
                }
                bool isLast = (i == count - 1);
                if (!isLast)
                    SqlBuilder.AppendLine(",");
                else if (appendLine)
                    SqlBuilder.AppendLine();
            }

            SqlBuilder.IndentLevel--;
        }

        private void GenerateFromClause(DbFromClause from)
        {
            bool isDbTable = from.GetType() == typeof(DbTableClause);
            
            SqlBuilder.Append("FROM");
            if (isDbTable)
                SqlBuilder.Append(" ");
            else
                SqlBuilder.AppendLine().IndentLevel++;

            from.Accept(this);

            if (!isDbTable)
                SqlBuilder.IndentLevel--;
        }

        private void GenerateWhereClause(DbExpression where)
        {
            if (where == null)
                return;

            SqlBuilder.AppendLine().Append("WHERE ");
            where.Accept(_expressionGenerator);
        }

        private void GenerateGroupByClause(IReadOnlyList<DbExpression> groupBy)
        {
            if (groupBy == null || groupBy.Count == 0)
                return;

            SqlBuilder.AppendLine().Append("GROUP BY");
            GenerateExpressionList(groupBy.Count, i => groupBy[i], null, false);
        }

        private void GenerateHavingClause(DbExpression having)
        {
            if (having == null)
                return;

            SqlBuilder.AppendLine().Append("HAVING ");
            having.Accept(_expressionGenerator);
        }

        private void GenerateOrderByClause(IReadOnlyList<DbExpressionSort> orderByList, int offset, int fetch)
        {
            if (orderByList == null)
                return;

            SqlBuilder.AppendLine().Append("ORDER BY ");
            for (int i = 0; i < orderByList.Count; i++)
            {
                var orderBy = orderByList[i];
                orderBy.Expression.Accept(_expressionGenerator);
                var direction = orderBy.Direction;
                if (direction == SortDirection.Descending)
                    SqlBuilder.Append(" DESC");
                else if (direction == SortDirection.Ascending)
                    SqlBuilder.Append(" ASC");
                bool isLast = i == orderByList.Count - 1;
                if (!isLast)
                    SqlBuilder.Append(", ");
            }

            if (offset == -1)
                return;

            SqlBuilder.AppendLine();
            SqlBuilder.Append("OFFSET ");
            SqlBuilder.Append(offset.ToString(NumberFormatInfo.InvariantInfo));
            SqlBuilder.Append(" ROWS");

            if (fetch != -1)
            {
                SqlBuilder.Append(" FETCH NEXT ");
                SqlBuilder.Append(fetch.ToString(NumberFormatInfo.InvariantInfo));
                SqlBuilder.Append(" ROWS ONLY");
            }
        }

        public int CommandParametersCount
        {
            get { return _expressionGenerator.ParametersCount; }
        }

        public IEnumerable<SqlParameter> CommandParameters
        {
            get
            {
                for (int i = 0; i < _expressionGenerator.ParametersCount; i++)
                    yield return _expressionGenerator.CreateSqlParameter(i);
            }
        }

        public string CommandText
        {
            get { return SqlBuilder.ToString(); }
        }

        public SqlCommand CreateCommand(SqlConnection sqlConnection, params SqlParameter[] extraParameters)
        {
            var commandParameters = extraParameters == null ? CommandParameters : CommandParameters.Concat(extraParameters.Where(x => x != null));
            return CommandText.CreateSqlCommand(sqlConnection, commandParameters);
        }
    }
}
