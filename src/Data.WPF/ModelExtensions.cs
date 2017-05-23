﻿using DevZest.Data;
using System;

namespace DevZest.Windows
{
    public static class ModelExtensions
    {
        public static DataRowFilter Where<T>(this T _, Func<T, DataRow, bool> condition)
            where T : Model
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (condition.Target != null)
                throw new ArgumentException(Strings.ModelExtensions_ExpressionMustBeStatic, nameof(condition));

            return DataRowFilter.Create(condition);
        }
    }
}
