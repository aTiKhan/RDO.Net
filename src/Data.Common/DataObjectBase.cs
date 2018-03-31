﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public abstract class DataObjectBase
    {
        private sealed class _Model : Model
        {
        }

        private readonly _Model _model = new _Model();
        private DataSet<_Model> _dataSet;

        protected T CreateColumn<T>(string name, Action<T> initializer = null)
            where T : Column, new()
        {
            Check.NotEmpty(name, nameof(name));

            var result = new T();
            if (initializer != null)
                initializer(result);
            result.Initialize(_model, typeof(_Model), name, ColumnKind.ModelProperty, null);
            return result;
        }

        private void EnsureDataSetInitialized()
        {
            if (_dataSet == null)
            {
                _dataSet = DataSet<_Model>.Create(_model);
                _dataSet.AddRow();
            }
        }

        private void VerifyColumn<T>(Column<T> column, string paramName)
        {
            Check.NotNull(column, paramName);

            if (column.ParentModel == null || column.ParentModel != _model)
                throw new ArgumentException(DiagnosticMessages.Scalar_InvalidColumn, paramName);
        }

        protected T GetValue<T>(Column<T> column)
        {
            VerifyColumn(column, nameof(column));
            EnsureDataSetInitialized();
            return column[0];
        }

        protected void SetValue<T>(Column<T> column, T value)
        {
            VerifyColumn(column, nameof(column));
            EnsureDataSetInitialized();
            column[0] = value;
        }

        public override string ToString()
        {
            return ToJsonString(true);
        }

        public string ToJsonString(bool isPretty)
        {
            EnsureDataSetInitialized();
            return _dataSet.ToJsonString(isPretty);
        }

        public static T ParseJson<T>(string json)
            where T : DataObjectBase, new()
        {
            Check.NotEmpty(json, nameof(json));

            var result = new T();
            var dataSet = result._dataSet;
            if (dataSet != null)
                dataSet.RemoveAt(0);
            new JsonParser(json).Parse(result._dataSet, true);
            return dataSet.Count == 1 ? result : null;
        }
    }
}