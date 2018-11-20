﻿using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;
using System.ComponentModel;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [AttributeSpec(addonTypes: new Type[] { typeof(ColumnDefault) }, validOnTypes: new Type[] { typeof(Column) })]
    public sealed class DefaultValueAttribute : ColumnAttribute
    {
        public DefaultValueAttribute(Type type, string value)
        {
            _defaultValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
        }

        public DefaultValueAttribute(bool value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(byte value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(short value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(int value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(long value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(char value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(double value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(float value)
        {
            _defaultValue = value;
        }

        public DefaultValueAttribute(string value)
        {
            _defaultValue = value;
        }

        private readonly object _defaultValue;

        public string Name { get; set; }

        public string Description { get; set; }

        protected override void Wireup(Column column)
        {
            column.SetDefaultObject(TypeDescriptor.GetConverter(column.DataType).ConvertFrom(_defaultValue), Name, Description);
        }
    }
}
