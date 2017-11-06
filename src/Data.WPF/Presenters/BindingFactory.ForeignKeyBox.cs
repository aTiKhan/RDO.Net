﻿using DevZest.Data.Views;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension>(this TKey key, TExtension extension, Func<ColumnValueBag, TKey, string> toString)
            where TKey : KeyBase
            where TExtension : ModelExtension
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return AsForeignKeyBox(key, extension, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TExtension paramExt) =>
            {
                v.Text = toString(valueBag, paramKey);
            });
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension>(this TKey key, TExtension extension, Func<ColumnValueBag, TKey, TExtension, string> toString)
            where TKey : KeyBase
            where TExtension : ModelExtension
        {
            if (toString == null)
                throw new ArgumentNullException(nameof(toString));

            return AsForeignKeyBox(key, extension, (TextBlock v, ColumnValueBag valueBag, TKey paramKey, TExtension paramExt) =>
            {
                v.Text = toString(valueBag, paramKey, paramExt);
            });
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension, TView>(this TKey key, TExtension extension, Action<TView, ColumnValueBag, TExtension> refreshAction)
            where TKey : KeyBase
            where TExtension : ModelExtension
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) =>
                {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extension = extension;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Extension);
                    refreshAction((TView)v.Content, v.ValueBag, extension);
                    },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, extension);
        }

        public static RowBinding<ForeignKeyBox> AsForeignKeyBox<TKey, TExtension, TView>(this TKey key, TExtension extension, Action<TView, ColumnValueBag, TKey, TExtension> refreshAction)
            where TKey : KeyBase
            where TExtension : ModelExtension
            where TView : UIElement, new()
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (refreshAction == null)
                throw new ArgumentNullException(nameof(refreshAction));

            return new RowBinding<ForeignKeyBox>(
                onSetup: (v, p) => {
                    v.Content = new TView();
                    v.ForeignKey = key;
                    v.Extension = extension;
                },
                onRefresh: (v, p) => {
                    p.SetValueBag(v.ValueBag, v.ForeignKey, v.Extension);
                    refreshAction((TView)v.Content, v.ValueBag, key, extension);
                },
                onCleanup: (v, p) => {
                    v.Content = null;
                }).WithInput(key, extension);
        }

        private static RowBinding<ForeignKeyBox> WithInput(this RowBinding<ForeignKeyBox> rowBinding, KeyBase foreignKey, ModelExtension extension)
        {
            var rowInput = rowBinding.BeginInput(ForeignKeyBox.ValueBagProperty);
            foreach (var columnSort in foreignKey)
                rowInput.WithFlush(columnSort.Column, v => v.ValueBag);
            if (extension != null)
            {
                foreach (var column in extension.Columns)
                {
                    if (column.IsExpression)
                        continue;
                    rowInput.WithFlush(column, v => v.ValueBag);
                }
            }
            return rowInput.EndInput();
        }
    }
}
