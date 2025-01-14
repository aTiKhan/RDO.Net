﻿using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a string column.
    /// </summary>
    public sealed class _String : Column<String>, IColumn<DbReader, String>
    {
        /// <inheritdoc/>
        public override bool AreEqual(string x, string y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<string> CreateParam(string value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<string> CreateConst(string value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(String value)
        {
            return JsonValue.String(value);
        }

        /// <inheritdoc/>
        protected internal override String DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : value.Text;
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public String this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private String GetValue(DbReader reader)
        {
            return reader.GetString(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc />
        protected internal override bool IsNull(string value)
        {
            return value == null;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _String Param(String x, _String sourceColumn = null)
        {
            return new ParamExpression<String>(x, sourceColumn).MakeColumn<_String>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _String Const(String x)
        {
            return new ConstantExpression<String>(x).MakeColumn<_String>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _String(String x)
        {
            return Param(x);
        }

        private sealed class AddExpression : BinaryExpression<String>
        {
            public AddExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override String EvalCore(String x, String y)
            {
                if (x != null && y != null)
                    return x + y;
                else
                    return null;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_String" /> objects.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _String operator +(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AddExpression(x, y).MakeColumn<_String>();
        }

        private sealed class LessThanExpression : BinaryExpression<String, bool?>
        {
            public LessThanExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            public LessThanOrEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <=(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<String, bool?>
        {
            public GreaterThanExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            public GreaterThanOrEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >=(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<String, bool?>
        {
            public EqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) == 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator ==(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<String, bool?>
        {
            public NotEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) != 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator !=(_String x, _String y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        /// <exclude />
        public override bool Equals(object obj)
        {
            // override to eliminate compile warning
            return base.Equals(obj);
        }

        /// <exclude />
        public override int GetHashCode()
        {
            // override to eliminate compile warning
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return this;
        }
    }
}
