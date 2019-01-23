﻿using System;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.MySql.Addons;

namespace DevZest.Data.MySql
{
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlBinaryAttribute : ColumnAttribute
    {
        public MySqlBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsMySqlBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlCharAttribute : ColumnAttribute
    {
        public MySqlCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDate();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDateTime();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlDecimalAttribute : ColumnAttribute
    {
        public SqlDecimalAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public byte Precision { get; private set; }

        public byte Scale { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsMySqlDecimal(Precision, Scale);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharAttribute : ColumnAttribute
    {
        public SqlNCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlNChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharAttribute : ColumnAttribute
    {
        public SqlNVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlNVarChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlTime();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsMySqlTimestamp();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryAttribute : ColumnAttribute
    {
        public SqlVarBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsMySqlVarBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharAttribute : ColumnAttribute
    {
        public SqlVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlVarChar(Size);
        }
    }
}
