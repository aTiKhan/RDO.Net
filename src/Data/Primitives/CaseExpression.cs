﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE WHEN..ELSE... expression.</summary>
    /// <typeparam name="TResult">The data type of the result.</typeparam>
    public sealed class CaseExpression<TResult> : ColumnExpression<TResult>
    {
        internal CaseExpression()
        {
            _when = new List<_Boolean>();
            _then = new List<Column<TResult>>();
        }

        private CaseExpression(List<_Boolean> when, List<Column<TResult>> then, Column<TResult> elseExpr)
        {
            _when = when;
            _then = then;
            _else = elseExpr;
        }

        private List<_Boolean> _when;
        private List<Column<TResult>> _then;
        private Column<TResult> _else;

        /// <inheritdoc />
        protected sealed override IColumns GetBaseColumns()
        {
            var result = Columns.Empty;
            for (int i = 0; i < _when.Count; i++)
                result = result.Union(_when[i].BaseColumns);
            for (int i = 0; i < _then.Count; i++)
                result = result.Union(_then[i].BaseColumns);
            result = result.Union(_else.BaseColumns);
            return result.Seal();
        }

        /// <summary>
        /// Construct CASE WHEN expression.
        /// </summary>
        /// <param name="when">The condition.</param>
        /// <returns>The result.</returns>
        public CaseWhen<TResult> When(_Boolean when)
        {
            when.VerifyNotNull(nameof(when));
            return new CaseWhen<TResult>(this, when);
        }

        internal CaseExpression<TResult> WhenThen(_Boolean when, Column<TResult> then)
        {
            Debug.Assert(!object.ReferenceEquals(when, null));
            Debug.Assert(then != null);
            _when.Add(when);
            _then.Add(then);
            return this;
        }

        /// <summary>Builds the ELSE... expression.</summary>
        /// <typeparam name="TColumn">The column type of the result.</typeparam>
        /// <param name="value">Value of the Else... expression.</param>
        /// <returns>The result column expression.</returns>
        public TColumn Else<TColumn>(TColumn value)
            where TColumn : Column<TResult>, new()
        {
            _else = value;
            return this.MakeColumn<TColumn>();
        }

        /// <inheritdoc/>
        public sealed override TResult this[DataRow dataRow]
        {
            get
            {
                for (int i = 0; i < _when.Count; i++)
                {
                    var whenValue = _when[i][dataRow];
                    if (whenValue == true)
                        return _then[i][dataRow];
                }

                return _else[dataRow];
            }
        }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            return new DbCaseExpression(typeof(TResult), null,
                _when.Select(x => x.DbExpression),
                _then.Select(x => x.DbExpression),
                _else.DbExpression);
        }

        /// <inheritdoc/>
        protected override IModels GetScalarSourceModels()
        {
            var result = Models.Empty;
            for (int i = 0; i < _when.Count; i++)
            {
                result = result.Union(_when[i].ScalarSourceModels);
                result = result.Union(_then[i].ScalarSourceModels);
            }
            result = result.Union(_else.ScalarSourceModels);
            return result.Seal();
        }

        /// <inheritdoc/>
        protected override IModels GetAggregateBaseModels()
        {
            var result = Models.Empty;
            for (int i = 0; i < _when.Count; i++)
            {
                result = result.Union(_when[i].AggregateSourceModels);
                result = result.Union(_then[i].AggregateSourceModels);
            }
            result = result.Union(_else.AggregateSourceModels);
            return result;
        }

        /// <inheritdoc />
        protected internal override ColumnExpression PerformTranslateTo(Model model)
        {
            var when = _when.TranslateToColumns(model);
            var then = _then.TranslateToColumns(model);
            var elseExpr = _else.TranslateTo(model);
            if (when != _when || _then != then || elseExpr != _else)
                return new CaseExpression<TResult>(when, then, elseExpr);
            else
                return this;
        }
    }
}
