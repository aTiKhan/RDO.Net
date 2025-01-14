﻿using DevZest.Data.Primitives;
using System;

namespace DevZest.Data.SqlServer
{
    public static partial class SqlFunctions
    {
        private sealed class NotLikeFunction : ScalarFunctionExpression<bool?>
        {
            public NotLikeFunction(_String source, _String pattern)
                : base(source, pattern)
            {
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.NotLike; }
            }

            public override bool? this[DataRow dataRow]
            {
                get { throw new NotSupportedException("NotLike function is not supported by DataSet."); }
            }
        }

        /// <summary>
        /// Determines whether a specific character string does not match a specified pattern.
        /// </summary>
        /// <param name="source">The source string column.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns><see cref="_Boolean"/> column that contains the result.</returns>
        public static _Boolean NotLike(this _String source, _String pattern)
        {
            source.VerifyNotNull(nameof(source));
            pattern.VerifyNotNull(nameof(pattern));
            return new NotLikeFunction(source, pattern).MakeColumn<_Boolean>();
        }
    }
}
