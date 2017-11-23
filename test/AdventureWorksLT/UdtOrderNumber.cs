﻿using System;
using DevZest.Data;
using DevZest.Data.SqlServer;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class UdtOrderNumber : UdtAttribute
    {
        public override Type DataType
        {
            get { return typeof(string); }
        }

        protected override void Initialize(Column column)
        {
            column.Nullable(true);
            ((Column<string>)column).AsNVarChar(25);
        }
    }
}
