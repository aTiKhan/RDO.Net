﻿using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RegularExpressionAttribute : GeneralValidationColumnAttribute
    {
        public RegularExpressionAttribute(string pattern)
        {
            Check.NotEmpty(pattern, nameof(pattern));
            Pattern = pattern;
        }

        public string Pattern { get; private set; }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return IsValid(column.GetValue(dataRow));
        }

        private Regex Regex { get; set; }

        private bool IsValid(object value)
        {
            SetupRegex();
            string text = Convert.ToString(value, CultureInfo.CurrentCulture);
            if (string.IsNullOrEmpty(text))
                return true;
            Match match = Regex.Match(text);
            return match.Success && match.Index == 0 && match.Length == text.Length;
        }

        private void SetupRegex()
        {
            if (Regex == null)
                Regex = new Regex(this.Pattern);
        }

        protected override string GetDefaultMessage(Column column, DataRow dataRow)
        {
            return Strings.RegularExpressionAttribute_DefaultErrorMessage(column.DisplayName, Pattern);
        }
    }
}