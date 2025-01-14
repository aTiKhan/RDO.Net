﻿using DevZest.Data.Primitives;
using System;
using System.Reflection;

namespace DevZest.Data
{
    public static partial class Functions
    {
        #region Min

        private sealed class ComparableMinFunction<T> : AggregateFunctionExpression<T>
            where T : IComparable<T>
        {
            public ComparableMinFunction(Column<T> x)
                : base(x)
            {
                _column = x;
            }

            private Column<T> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Min; }
            }

            T result;
            protected override void EvalInit()
            {
                result = default(T);
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                var value = _column[dataRow];
                if (value == null)
                    return;

                if (result == null || value.CompareTo(result) < 0)
                    result = value;
            }

            protected override T EvalReturn()
            {
                return result;
            }
        }

        private sealed class NullableMinFunction<T> : AggregateFunctionExpression<Nullable<T>>
            where T : struct, IComparable<T>
        {
            public NullableMinFunction(Column<Nullable<T>> x)
                : base(x)
            {
                _column = x;
            }

            private Column<Nullable<T>> _column;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Min; }
            }

            T? result;
            protected override void EvalInit()
            {
                result = null;
            }

            protected override void EvalAccumulate(DataRow dataRow)
            {
                var value = _column[dataRow];

                if (!value.HasValue)
                    return;

                if (!result.HasValue || value.GetValueOrDefault().CompareTo(result.GetValueOrDefault()) < 0)
                    result = value;
            }

            protected override T? EvalReturn()
            {
                return result;
            }
        }

        private sealed class ComparableMinFunctionInvoker<T> : ColumnInvoker<T>
            where T : Column
        {
            public static readonly ComparableMinFunctionInvoker<T> Singleton = new ComparableMinFunctionInvoker<T>();

            private ComparableMinFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(ComparableMin)))
            {
            }
        }

        private sealed class NullableMinFunctionInvoker<T> : ColumnInvoker<T>
            where T : Column
        {
            public static readonly NullableMinFunctionInvoker<T> Singleton = new NullableMinFunctionInvoker<T>();

            private NullableMinFunctionInvoker()
                : base(typeof(Functions).GetStaticMethodInfo(nameof(NullableMin)), true)
            {
            }
        }

        /// <summary>
        /// Gets the minimum value in the column.
        /// </summary>
        /// <typeparam name="T">Type of the column.</typeparam>
        /// <param name="x">The column.</param>
        /// <returns>The result contains the minimum value.</returns>
        public static T Min<T>(this T x)
            where T : Column
        {
            bool? bypassNullable = BypassNullableToComparable<T>();
            if (!bypassNullable.HasValue)
                throw new NotSupportedException();

            if (bypassNullable.GetValueOrDefault())
                return NullableMinFunctionInvoker<T>.Singleton.Invoke(x);
            else
                return ComparableMinFunctionInvoker<T>.Singleton.Invoke(x);
        }

        private static bool? BypassNullableToComparable<T>()
            where T : Column
        {
            for (var type = typeof(T); type != null; type = type.GetTypeInfo().BaseType)
            {
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Column<>))
                {
                    var typeParam = type.GetGenericArguments()[0];
                    if (typeParam.IsComparable())
                        return false;

                    if (typeParam.GetTypeInfo().IsGenericType && typeParam.GetGenericTypeDefinition() == typeof(Nullable<>))
                        typeParam = typeParam.GetGenericArguments()[0];

                    return typeParam.IsComparable() ? new bool?(true) : null;
                }
            }

            return null;
        }

        private static bool IsComparable(this Type type)
        {
            return typeof(IComparable).IsAssignableFrom(type);
        }

        private static T ComparableMin<T, TValue>(this T x)
            where T : Column<TValue>, new()
            where TValue : IComparable<TValue>
        {
            return new ComparableMinFunction<TValue>(x).MakeColumn<T>();
        }

        private static T NullableMin<T, TValue>(this T x)
            where T : Column<Nullable<TValue>>, new()
            where TValue : struct, IComparable<TValue>
        {
            return new NullableMinFunction<TValue>(x).MakeColumn<T>();
        }

        #endregion
    }
}
