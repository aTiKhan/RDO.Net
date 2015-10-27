﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace DevZest.Data
{
    public abstract class ModelKey : IReadOnlyCollection<ColumnSort>
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        protected internal sealed class SortAttribute : Attribute
        {
            public SortAttribute(SortDirection direction)
            {
                Direction = direction;
            }

            public SortDirection Direction { get; private set; }
        }

        internal ReadOnlyCollection<ColumnMapping> GetColumnMappings(ModelKey target)
        {
            return new ReadOnlyCollection<ColumnMapping>(GetColumnMappings(this, target));
        }

        private static IList<ColumnMapping> GetColumnMappings(ModelKey source, ModelKey target)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);
            Debug.Assert(source.Count == target.Count);

            var result = new List<ColumnMapping>();
            for (int i = 0; i < source.Count; i++)
            {
                var sourceColumn = source[i].Column;
                var targetColumn = target[i].Column;
                result.Add(targetColumn.CreateMapping(sourceColumn));
            }

            return result;
        }

        internal ModelKey Clone(SequentialKeyModel model)
        {
            EnsureIntialized();
            var result = (ModelKey)this.MemberwiseClone();
            result._parentModel = model;
            result._columns = new ColumnSort[_columns.Length];
            for (int i = 0; i < Count; i++)
            {
                var columnSort = _columns[i];
                result._columns[i] = new ColumnSort(columnSort.Column.Clone(model), columnSort.Direction);
            }
            return result;
        }

        private struct ColumnGetter
        {
            public ColumnGetter(string name, Func<ModelKey, Column> invoker, SortAttribute sort)
            {
                Name = name;
                Invoker = invoker;
                Sort = sort;
            }

            public readonly string Name;
            public readonly Func<ModelKey, Column> Invoker;
            public readonly SortAttribute Sort;
        }

        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<ColumnGetter>> s_columnGetters =
            new ConcurrentDictionary<Type, ReadOnlyCollection<ColumnGetter>>();
        protected ModelKey()
        {
        }

        internal ColumnSort[] _columns;
        private bool IsInitialized
        {
            get { return _columns != null; }
        }

        public int Count
        {
            get
            {
                EnsureIntialized();
                return _columns.Length;
            }
        }

        public ColumnSort this[int index]
        {
            get
            {
                EnsureIntialized();
                return _columns[index];
            }
        }

        public bool Contains(Column column)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Column == column)
                    return true;
            }
            return false;
        }

        private void EnsureIntialized()
        {
            if (IsInitialized)
                return;

            var columnGetters = GetColumnGetters(this.GetType());
            _columns = new ColumnSort[columnGetters.Count];
            for (int i = 0; i < columnGetters.Count; i++)
            {
                var column = columnGetters[i].Invoker(this);
                if (column == null)
                    throw new InvalidOperationException(Strings.ColumnGroup_GetterReturnsNull(this.GetType().FullName, columnGetters[i].Name));
                if (i == 0)
                    _parentModel = column.ParentModel;
                else if (_parentModel != column.ParentModel)
                    throw new InvalidOperationException(Strings.ColumnGroup_InconsistentParentModel(this.GetType().FullName, columnGetters[0].Name, columnGetters[i].Name));

                var sort = columnGetters[i].Sort;
                _columns[i] = new ColumnSort(column, sort == null ? SortDirection.Unspecified : sort.Direction);
            }
        }

        private Model _parentModel;
        internal Model ParentModel
        {
            get
            {
                EnsureIntialized();
                return _parentModel;
            }
        }

        private static ReadOnlyCollection<ColumnGetter> GetColumnGetters(Type type)
        {
            ReadOnlyCollection<ColumnGetter> result;
            if (!s_columnGetters.TryGetValue(type, out result))
            {
                // It is possible that multiple threads will enter this code to resolve the
                // column getters.  However, the result will always be the same so we may, in
                // the rare cases in which this happens, do some work twice, but functionally the
                // outcome will be correct.
                var definitions = ResolveColumnGetters(type);
                result = new ReadOnlyCollection<ColumnGetter>(definitions);

                // If TryAdd fails it just means some other thread got here first, which is okay
                // since the end result is the same info anyway.
                s_columnGetters.TryAdd(type, result);
            }
            return result;
        }

        internal static int GetColumnCount(Type type)
        {
            return GetColumnGetters(type).Count;
        }

        private static List<ColumnGetter> ResolveColumnGetters(Type type)
        {
            var result = new List<ColumnGetter>();

            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                if (!typeof(Column).IsAssignableFrom(propertyInfo.PropertyType))
                    continue;

                if (propertyInfo.GetIndexParameters().Length != 0)
                    continue;

                var getMethod = propertyInfo.GetMethod;
                if (getMethod == null || getMethod.IsStatic || !getMethod.IsPublic)
                    continue;
                result.Add(GetColumnGetter(propertyInfo, getMethod));
            }
            return result;
        }

        private static ColumnGetter GetColumnGetter(PropertyInfo propertyInfo, MethodInfo getMethod)
        {
            var modelKeyParam = Expression.Parameter(typeof(ModelKey), "modelKey");

            var getterInstance = Expression.Convert(modelKeyParam, getMethod.DeclaringType);
            var getterExpression = Expression.Call(getterInstance, getMethod);
            var getter = Expression.Lambda<Func<ModelKey, Column>>(getterExpression, modelKeyParam).Compile();

            var sort = propertyInfo.GetCustomAttribute<SortAttribute>();
            return new ColumnGetter(getMethod.Name, getter, sort);
        }

        public IEnumerator<ColumnSort> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
    }
}