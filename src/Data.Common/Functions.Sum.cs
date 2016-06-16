﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Sum

        private abstract class SumBase<T> : AggregateFunctionExpression<T>
        {
            protected SumBase(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            private Column<T> _column;

            protected sealed override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Sum; }
            }

            protected abstract void EvalAccumulate(T value);

            protected sealed override void EvalAccumulate(DataRow dataRow)
            {
                EvalAccumulate(_column[dataRow]);
            }
        }

        [ExpressionConverterNonGenerics(typeof(SumInt32.Converter), TypeId = "Sum(_Int32)")]
        private sealed class SumInt32 : SumBase<Int32?>
        {
            private sealed class Converter : ConverterBase<Column<Int32?>, SumInt32>
            {
                protected override SumInt32 MakeExpression(Column<int?> param)
                {
                    return new SumInt32(param);
                }
            }

            public SumInt32(Column<Int32?> x)
                : base(x)
            {
            }

            int count;
            int num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Int32? value)
            {
                checked
                {
                    if (value.HasValue)
                        num += value.GetValueOrDefault();
                    count++;
                }
            }

            protected override int? EvalReturn()
            {
                return count == 0? null : new int?(num);
            }
        }

        public static _Int32 Sum(this Column<Int32?> x)
        {
            Check.NotNull(x, nameof(x));
            return new SumInt32(x).MakeColumn<_Int32>();
        }

        [ExpressionConverterNonGenerics(typeof(SumInt64.Converter), TypeId = "Sum(_Int64)")]
        private sealed class SumInt64 : SumBase<Int64?>
        {
            private sealed class Converter : ConverterBase<Column<Int64?>, SumInt64>
            {
                protected override SumInt64 MakeExpression(Column<long?> param)
                {
                    return new SumInt64(param);
                }
            }

            public SumInt64(Column<Int64?> x)
                : base(x)
            {
            }

            int count;
            Int64 num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Int64? value)
            {
                checked
                {
                    if (value.HasValue)
                        num += value.GetValueOrDefault();
                    count++;
                }
            }

            protected override Int64? EvalReturn()
            {
                return count == 0 ? null : new Int64?(num);
            }
        }

        public static _Int64 Sum(this Column<Int64?> x)
        {
            Check.NotNull(x, nameof(x));
            return new SumInt64(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(SumDecimal.Converter), TypeId = "Sum(_Decimal)")]
        private sealed class SumDecimal : SumBase<Decimal?>
        {
            private sealed class Converter : ConverterBase<Column<Decimal?>, SumDecimal>
            {
                protected override SumDecimal MakeExpression(Column<decimal?> param)
                {
                    return new SumDecimal(param);
                }
            }

            public SumDecimal(Column<Decimal?> x)
                : base(x)
            {
            }

            int count;
            Decimal num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Decimal? value)
            {
                checked
                {
                    if (value.HasValue)
                        num += value.GetValueOrDefault();
                    count++;
                }
            }

            protected override Decimal? EvalReturn()
            {
                return count == 0 ? null : new Decimal?(num);
            }
        }

        public static _Decimal Sum(this Column<Decimal?> x)
        {
            Check.NotNull(x, nameof(x));
            return new SumDecimal(x).MakeColumn<_Decimal>();
        }

        [ExpressionConverterNonGenerics(typeof(SumDouble.Converter), TypeId = "Sum(_Double)")]
        private sealed class SumDouble : SumBase<Double?>
        {
            private sealed class Converter : ConverterBase<Column<Double?>, SumDouble>
            {
                protected override SumDouble MakeExpression(Column<double?> param)
                {
                    return new SumDouble(param);
                }
            }

            public SumDouble(Column<Double?> x)
                : base(x)
            {
            }

            int count;
            Double num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Double? value)
            {
                checked
                {
                    if (value.HasValue)
                        num += value.GetValueOrDefault();
                    count++;
                }
            }

            protected override Double? EvalReturn()
            {
                return count == 0 ? null : new Double?(num);
            }
        }

        public static _Double Sum(this Column<Double?> x)
        {
            Check.NotNull(x, nameof(x));
            return new SumDouble(x).MakeColumn<_Double>();
        }

        [ExpressionConverterNonGenerics(typeof(SumSingle.Converter), TypeId = "Sum(_Single)")]
        private sealed class SumSingle : SumBase<Single?>
        {
            private sealed class Converter : ConverterBase<Column<Single?>, SumSingle>
            {
                protected override SumSingle MakeExpression(Column<float?> param)
                {
                    return new SumSingle(param);
                }
            }

            public SumSingle(Column<Single?> x)
                : base(x)
            {
            }

            int count;
            Single num;

            protected override void EvalInit()
            {
                count = 0;
                num = 0;
            }

            protected override void EvalAccumulate(Single? value)
            {
                checked
                {
                    if (value.HasValue)
                        num += value.GetValueOrDefault();
                    count++;
                }
            }

            protected override Single? EvalReturn()
            {
                return count == 0 ? null : new Single?(num);
            }
        }

        public static _Single Sum(this Column<Single?> x)
        {
            Check.NotNull(x, nameof(x));
            return new SumSingle(x).MakeColumn<_Single>();
        }

        #endregion
    }
}
