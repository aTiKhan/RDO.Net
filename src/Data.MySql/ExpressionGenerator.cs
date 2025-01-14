﻿using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using MySql.Data.MySqlClient;

namespace DevZest.Data.MySql
{
    /// <summary>
    /// Generates SQL from expression.
    /// </summary>
    public sealed class ExpressionGenerator : DbExpressionVisitor
    {
        private const string NULL = "NULL";

        /// <summary>
        /// Gets the string builder for T-SQL generation.
        /// </summary>
        public IndentedStringBuilder SqlBuilder { get; internal set; }

        internal IModelAliasManager ModelAliasManager { get; set; }

        /// <summary>
        /// Gets the MySQL version.
        /// </summary>
        public MySqlVersion MySqlVersion { get; internal set; }

        private static readonly Dictionary<BinaryExpressionKind, string> BinaryExpressionMappers = new Dictionary<BinaryExpressionKind, string>()
        {
            { BinaryExpressionKind.Add, " + " },
            { BinaryExpressionKind.And, " AND " },
            { BinaryExpressionKind.BitwiseAnd, " & " },
            { BinaryExpressionKind.BitwiseOr, " | " },
            { BinaryExpressionKind.BitwiseXor, " ^ " },
            { BinaryExpressionKind.Divide, " / " },
            { BinaryExpressionKind.Equal, " = " },
            { BinaryExpressionKind.GreaterThan, " > " },
            { BinaryExpressionKind.GreaterThanOrEqual, " >= " },
            { BinaryExpressionKind.LessThan, " < " },
            { BinaryExpressionKind.LessThanOrEqual, " <= " },
            { BinaryExpressionKind.Modulo, " % " },
            { BinaryExpressionKind.Multiply, " * " },
            { BinaryExpressionKind.NotEqual, " <> " },
            { BinaryExpressionKind.Or, " OR " },
            { BinaryExpressionKind.Substract, " - " },
        };

        /// <inheritdoc/>
        public override void Visit(DbBinaryExpression e)
        {
            if (e.Left.DataType == typeof(string) && e.Right.DataType == typeof(string))
            {
                SqlBuilder.Append("CONCAT(");
                e.Left.Accept(this);
                SqlBuilder.Append(", ");
                e.Right.Accept(this);
                SqlBuilder.Append(")");
                return;
            }

            SqlBuilder.Append("(");
            e.Left.Accept(this);
            SqlBuilder.Append(BinaryExpressionMappers[e.Kind]);
            e.Right.Accept(this);
            SqlBuilder.Append(")");
        }

        /// <inheritdoc/>
        public override void Visit(DbCaseExpression e)
        {
            SqlBuilder.Append("CASE");
            if (e.On != null)
            {
                SqlBuilder.Append(' ');
                e.On.Accept(this);
            }
            SqlBuilder.AppendLine();

            for (var i = 0; i < e.When.Count; ++i)
            {
                SqlBuilder.IndentLevel++;
                SqlBuilder.Append("WHEN ");
                e.When[i].Accept(this);
                SqlBuilder.Append(" THEN ");
                e.Then[i].Accept(this);
                SqlBuilder.AppendLine();
                SqlBuilder.IndentLevel--;
            }

            SqlBuilder.IndentLevel++;
            SqlBuilder.Append("ELSE ");
            e.Else.Accept(this);
            SqlBuilder.AppendLine();
            SqlBuilder.IndentLevel--;

            SqlBuilder.Append("END CASE");
        }

        /// <inheritdoc/>
        public override void Visit(DbCastExpression e)
        {
            var sourceSqlType = e.SourceColumn.GetMySqlType();
            var targetSqlType = e.TargetColumn.GetMySqlType();
            if (CanEliminateCast(sourceSqlType, targetSqlType))
                e.Operand.Accept(this);
            else
            {
                SqlBuilder.Append("CAST(");
                e.Operand.Accept(this);
                SqlBuilder.Append(" AS ");
                SqlBuilder.Append(targetSqlType.GetCastAsType(MySqlVersion));
                SqlBuilder.Append(")");
            }
        }

        private bool CanEliminateCast(MySqlType sourceMySqlType, MySqlType targetMySqlType)
        {
            return sourceMySqlType.GetSqlParameterInfo(MySqlVersion).MySqlDbType == targetMySqlType.GetSqlParameterInfo(MySqlVersion).MySqlDbType;
        }

        /// <inheritdoc/>
        public override void Visit(DbColumnExpression e)
        {
            var columnName = e.DbColumnName.ToQuotedIdentifier();
            if (ModelAliasManager != null)
            {
                var modelAlias = ModelAliasManager[e.Column.GetParent()].ToQuotedIdentifier();
                SqlBuilder.Append(modelAlias).Append('.').Append(columnName);
            }
            else
                SqlBuilder.Append(columnName);
        }

        /// <inheritdoc/>
        public override void Visit(DbConstantExpression e)
        {
            GenerateConst(SqlBuilder, MySqlVersion, e.Column, e.Value);
        }

        internal static void GenerateConst(IndentedStringBuilder sqlBuilder, MySqlVersion mySqlVersion, Column column, object value)
        {
            if (value == null)
            {
                sqlBuilder.Append(NULL);
                return;
            }

            sqlBuilder.Append(column.GetMySqlType().GetLiteral(value, mySqlVersion));
        }

        private static Dictionary<FunctionKey, Action<ExpressionGenerator, DbFunctionExpression>> s_functionHandlers = new Dictionary<FunctionKey, Action<ExpressionGenerator, DbFunctionExpression>>()
        {
            { FunctionKeys.IsNull, (g, e) => g.VisitFunction_IsNull(e) },
            { FunctionKeys.IsNotNull, (g, e) => g.VisitFunction_IsNotNull(e) },
            { FunctionKeys.IfNull, (g, e) => g.VisitFunction_IfNull(e) },
            { FunctionKeys.Now, (g, e) => g.VisitFunction_Now(e) },
            { FunctionKeys.UtcNow, (g, e) => g.VisitFunction_UtcNow(e) },
            { FunctionKeys.NewGuid, (g, e) => g.VisitFunction_NewGuid(e) },
            { FunctionKeys.Average, (g, e) => g.VisitFunction_Average(e) },
            { FunctionKeys.Count, (g, e) => g.VisitFunction_Count(e) },
            { FunctionKeys.CountRows, (g, e) => g.VisitFunction_CountRows(e) },
            { FunctionKeys.First, (g, e) => g.VisitFunction_First(e) },
            { FunctionKeys.Last, (g, e) => g.VisitFunction_Last(e) },
            { FunctionKeys.Max, (g, e) => g.VisitFunction_Max(e) },
            { FunctionKeys.Min, (g, e) => g.VisitFunction_Min(e) },
            { FunctionKeys.Sum, (g, e) => g.VisitFunction_Sum(e) },
            { FunctionKeys.Contains, (g, e) => g.VisitFunction_Contains(e) }
        };

        /// <summary>
        /// Registers handler to generate SQL for custom function.
        /// </summary>
        /// <param name="functionKey">The function key.</param>
        /// <param name="handler">The handler.</param>
        public static void RegisterFunctionHandler(FunctionKey functionKey, Action<ExpressionGenerator, DbFunctionExpression> handler)
        {
            functionKey.VerifyNotNull(nameof(functionKey));
            handler.VerifyNotNull(nameof(handler));

            s_functionHandlers.Add(functionKey, handler);
        }

        /// <inheritdoc/>
        public override void Visit(DbFunctionExpression e)
        {
            Action<ExpressionGenerator, DbFunctionExpression> handler;
            try
            {
                handler = s_functionHandlers[e.FunctionKey];
            }
            catch (KeyNotFoundException)
            {
                throw new NotSupportedException(DiagnosticMessages.FunctionNotSupported(e.FunctionKey));
            }
            handler(this, e);
        }

        private void VisitFunction_IsNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(" IS NULL)");
        }

        private void VisitFunction_IsNotNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(" IS NOT NULL)");
        }

        private void VisitFunction_IfNull(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 2);
            SqlBuilder.Append("IFNULL(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(", ");
            e.ParamList[1].Accept(this);
            SqlBuilder.Append(')');
        }

        private void VisitFunction_Now(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("NOW()");
        }

        private void VisitFunction_UtcNow(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("UTC_TIMESTAMP()");
        }

        private void VisitFunction_NewGuid(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 0);
            SqlBuilder.Append("UUID()");
        }

        private void VisitFunction_Average(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("AVG(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Count(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("COUNT(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_CountRows(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("COUNT(*)");
        }


        private void VisitFunction_First(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("FIRST(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Last(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("LAST(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Max(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("MAX(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Min(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("MIN(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Sum(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 1);
            SqlBuilder.Append("SUM(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(")");
        }

        private void VisitFunction_Contains(DbFunctionExpression e)
        {
            Debug.Assert(e.ParamList.Count == 2);
            SqlBuilder.Append("(INSTR(");
            e.ParamList[0].Accept(this);
            SqlBuilder.Append(", ");
            e.ParamList[1].Accept(this);
            SqlBuilder.Append(") > 0)");
        }

        private readonly List<DbParamExpression> DbParamExpressions = new List<DbParamExpression>();

        /// <summary>
        /// Gets the number of parameters.
        /// </summary>
        public int ParametersCount
        {
            get { return DbParamExpressions.Count; }
        }

        /// <summary>
        /// Gets the parameter expression at specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The parameter expression.</returns>
        public DbParamExpression GetDbParamExpression(int index)
        {
            return DbParamExpressions[index];
        }

        internal MySqlParameter CreateMySqlParameter(int index)
        {
            var dbParamExpression = DbParamExpressions[index];
            var column = dbParamExpression.SourceColumn ?? dbParamExpression.Column;
            var columnSqlType = column.GetMySqlType();
            return columnSqlType.CreateSqlParameter(GetParamName(index), ParameterDirection.Input, dbParamExpression.Value, MySqlVersion);
        }

        /// <inheritdoc/>
        public override void Visit(DbParamExpression e)
        {
            int index = DbParamExpressions.IndexOf(e);
            if (index == -1)
            {
                index = DbParamExpressions.Count;
                DbParamExpressions.Add(e);
            }
            string paramName = GetParamName(index);
            SqlBuilder.Append(paramName);
        }

        internal static string GetParamName(int index)
        {
            return "@p" + (index + 1).ToString(CultureInfo.InvariantCulture);
        }

        private static readonly Dictionary<DbUnaryExpressionKind, string> UnaryExpressionMappers = new Dictionary<DbUnaryExpressionKind, string>()
        {
            { DbUnaryExpressionKind.Negate, "-" },
            { DbUnaryExpressionKind.Not, "NOT " },
            { DbUnaryExpressionKind.OnesComplement, "~" }
        };

        /// <inheritdoc/>
        public override void Visit(DbUnaryExpression e)
        {
            SqlBuilder.Append("(");
            SqlBuilder.Append(UnaryExpressionMappers[e.Kind]);
            e.Operand.Accept(this);
            SqlBuilder.Append(")");
        }
    }
}
