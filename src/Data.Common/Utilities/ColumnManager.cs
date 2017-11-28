﻿using DevZest.Data.Annotations.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Utilities
{
    internal static class ColumnManager
    {
        internal static IEnumerable<ColumnAttribute> Verify<TParent, TColumn>(this Expression<Func<TParent, TColumn>> getter, string paramName)
        {
            Check.NotNull(getter, paramName);
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(Strings.InvalidGetterExpression, paramName);

            var name = memberExpr.Member.Name;
            var propertyInfo = typeof(TParent).GetProperty(name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.InvalidGetterExpression, paramName);

            var result = propertyInfo.GetCustomAttributes<ColumnAttribute>().ToArray();
            foreach (var columnAttribute in result)
                columnAttribute.DeclaringModelType = propertyInfo.DeclaringType;
            return result;
        }

        internal static Action<T> Merge<T>(this Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where T : Column, new()
        {
            return x =>
            {
                if (initializer != null)
                    initializer(x);
                InitializeColumnAttributes(x, columnAttributes);
            };
        }

        private static void InitializeColumnAttributes(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
                columnAttribute.TryInitialize(column);
        }
    }
}
