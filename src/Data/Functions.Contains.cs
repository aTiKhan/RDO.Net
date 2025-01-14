﻿using DevZest.Data.Primitives;

namespace DevZest.Data
{
    public static partial class Functions
    {
        private sealed class ContainsFunction : ScalarFunctionExpression<bool?>
        {
            public ContainsFunction(_String column, _String value)
                : base(column, value)
            {
                _column = column;
                _value = value;
            }

            private _String _column;
            private _String _value;

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Contains; }
            }

            public override bool? this[DataRow dataRow]
            {
                get
                {
                    var text = _column[dataRow];
                    var searchText = _value[dataRow];
                    if (text == null || searchText == null)
                        return null;
                    else
                        return text.Contains(searchText);
                }
            }
        }

        /// <summary>
        /// Determines whether column contains specified value.
        /// </summary>
        /// <param name="x">The column.</param>
        /// <param name="value">The specified value.</param>
        /// <returns>A <see cref="_Boolean"/> column which contains the result.</returns>
        public static _Boolean Contains(this _String x, _String value)
        {
            x.VerifyNotNull(nameof(x));
            value.VerifyNotNull(nameof(value));
            return new ContainsFunction(x, value).MakeColumn<_Boolean>();
        }
    }
}
