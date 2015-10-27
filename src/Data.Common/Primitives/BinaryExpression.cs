﻿using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents column expression which contains two column operands.</summary>
    /// <typeparam name="T">The data type of the column operands and column expression.</typeparam>
    public abstract class BinaryExpression<T> : BinaryExpression<T, T>
    {
        /// <summary>Initializes a new instance of <see cref="BinaryExpression{T}"/> class.</summary>
        /// <param name="left">The left column operand.</param>
        /// <param name="right">The right column operand.</param>
        protected BinaryExpression(Column<T> left, Column<T> right)
            : base(left, right)
        {
        }
    }

    /// <summary>Represents column expression which contains two column operands.</summary>
    /// <typeparam name="T">The data type of column operands.</typeparam>
    /// <typeparam name="TResult">The data type of the column expression.</typeparam>
    public abstract class BinaryExpression<T, TResult> : ColumnExpression<TResult>
    {
        /// <summary>Initializes a new instance of <see cref="BinaryExpression{T, TResult}"/> class.</summary>
        /// <param name="left">The left column operand.</param>
        /// <param name="right">The right column operand.</param>
        protected BinaryExpression(Column<T> left, Column<T> right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            Left = left;
            Right = right;
        }

        /// <summary>Gets the left column operand.</summary>
        public Column<T> Left { get; private set; }

        /// <summary>Gets the right column operand.</summary>
        public Column<T> Right { get; private set; }

        /// <summary>Gets the kind of this binary expression.</summary>
        protected abstract BinaryExpressionKind Kind { get; }

        /// <inheritdoc/>
        protected sealed override IModelSet GetParentModelSet()
        {
            return Left.ParentModelSet.Union(Right.ParentModelSet);
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetAggregateModelSet()
        {
            return Left.AggregateModelSet.Union(Right.AggregateModelSet);
        }

        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return new DbBinaryExpression(Kind, Left.DbExpression, Right.DbExpression);
        }

        /// <inheritdoc/>
        public sealed override TResult Eval(DataRow dataRow)
        {
            var x = Left.Eval(dataRow);
            var y = Right.Eval(dataRow);
            return EvalCore(x, y);
        }

        /// <summary>Evaluates the expression against two operand values.</summary>
        /// <param name="x">The left operand value.</param>
        /// <param name="y">The right operand value.</param>
        /// <returns>The result.</returns>
        protected abstract TResult EvalCore(T x, T y);

        internal sealed override Column GetParallelColumn(Model model)
        {
            var expr = (BinaryExpression<T, TResult>)this.MemberwiseClone();
            expr.Left = (Column<T>)Left.GetParrallel(model);
            expr.Right = (Column<T>)Right.GetParrallel(model);
            return expr.MakeColumn(Owner.GetType());
        }
    }
}