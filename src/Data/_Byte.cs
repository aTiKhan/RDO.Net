﻿using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Byte"/> column.
    /// </summary>
    public sealed class _Byte : Column<Byte?>, IColumn<DbReader, Byte?>
    {
        private sealed class CastToStringExpression : CastExpression<Byte?, String>
        {
            public CastToStringExpression(Column<Byte?> x)
                : base(x)
            {
            }

            protected override String Cast(Byte? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <inheritdoc />
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _String(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        protected sealed override Column<byte?> CreateParam(byte? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc />
        public override bool AreEqual(byte? x, byte? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected internal sealed override Column<byte?> CreateConst(byte? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Byte? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Byte? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Byte?(Convert.ToByte(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Byte? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Byte? GetValue(DbReader reader)
        {
            return reader.GetByte(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(byte? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Byte Param(byte? x, _Byte sourceColumn = null)
        {
            return new ParamExpression<byte?>(x, sourceColumn).MakeColumn<_Byte>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Byte Const(byte? x)
        {
            return new ConstantExpression<byte?>(x).MakeColumn<_Byte>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Byte(byte? x)
        {
            return Param(x);
        }

        private sealed class OnesComplementExpression : UnaryExpression<Byte?>
        {
            public OnesComplementExpression(Column<byte?> x)
                : base(x)
            {
            }

            protected override Byte? EvalCore(Byte? x)
            {
                return (Byte?)(~x);
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.OnesComplement; }
            }
        }

        /// <summary>Performs a bitwise one's complement operation on the specified <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression that contains the results of the one's complement operation.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static _Byte operator ~(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Byte>();
        }

        private sealed class AddExpression : BinaryExpression<Byte?>
        {
            public AddExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x + y);
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator +(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AddExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class SubstractExpression : BinaryExpression<Byte?>
        {
            public SubstractExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x - y);
            }
        }

        /// <summary>Substracts the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator -(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Byte?>
        {
            public MultiplyExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x * y);
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator *(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class DivideExpression : BinaryExpression<Byte?>
        {
            public DivideExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x / y);
            }
        }

        /// <summary>Divides the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator /(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class ModuloExpression : BinaryExpression<Byte?>
        {
            public ModuloExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x % y);
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Byte" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator %(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class BitwiseAndExpression : BinaryExpression<Byte?>
        {
            public BitwiseAndExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x & y);
            }
        }

        /// <summary>Computes the bitwise AND of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator &(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class BitwiseOrExpression : BinaryExpression<Byte?>
        {
            public BitwiseOrExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x | y);
            }
        }

        /// <summary>Computes the bitwise OR of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator |(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class BitwiseXorExpression : BinaryExpression<Byte?>
        {
            public BitwiseXorExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseXor; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x ^ y);
            }
        }

        /// <summary>Computes the bitwise exclusive-OR of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator ^(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class LessThanExpression : BinaryExpression<Byte?, bool?>
        {
            public LessThanExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator <(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public LessThanOrEqualExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator <=(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Byte?, bool?>
        {
            public GreaterThanExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator >(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator >=(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Byte?, bool?>
        {
            public EqualExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Byte" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator ==(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public NotEqualExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Byte" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator !=(_Byte x, _Byte y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromBooleanCast : CastExpression<bool?, Byte?>
        {
            public FromBooleanCast(Column<bool?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(bool? value)
            {
                if (value.HasValue)
                    return (Byte?)(value.GetValueOrDefault() ? 1 : 0);
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Byte(_Boolean x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Byte?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int16? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _Byte(_Int16 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, Byte?>
        {
            public FromInt32Cast(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int32? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _Byte(_Int32 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, Byte?>
        {
            public FromInt64Cast(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int64? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Byte(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromDecimalCast : CastExpression<Decimal?, Byte?>
        {
            public FromDecimalCast(Column<decimal?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Decimal? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _Byte(_Decimal x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Byte?>
        {
            public FromDoubleCast(Column<double?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Double? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Byte(_Double x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromSingleCast : CastExpression<Single?, Byte?>
        {
            public FromSingleCast(Column<Single?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(Single? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Byte(_Single x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromSingleCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromStringExpression : CastExpression<String, Byte?>
        {
            public FromStringExpression(Column<string> x)
                : base(x)
            {
            }

            protected override Byte? Cast(String value)
            {
                if (value == null)
                    return null;
                return Byte.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Byte(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringExpression(x).MakeColumn<_Byte>();
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
    }
}
