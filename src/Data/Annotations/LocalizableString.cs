﻿using System;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    internal class LocalizableString
    {
        private string _propertyName;
        private string _propertyValue;
        private Type _resourceType;
        private Func<string> _cachedResult;

        public string Value
        {
            get { return _propertyValue; }
            set
            {
                if (_propertyValue != value)
                {
                    ClearCache();
                    _propertyValue = value;
                }
            }
        }

        public Type ResourceType
        {
            get { return _resourceType; }
            set
            {
                if (_resourceType != value)
                {
                    ClearCache();
                    _resourceType = value;
                }
            }
        }

        public LocalizableString(string propertyName)
        {
            _propertyName = propertyName;
        }

        private void ClearCache()
        {
            _cachedResult = null;
        }

        public Func<string> LocalizableValueGetter
        {
            get
            {
                EnsureCachedResult();
                return string.IsNullOrEmpty(_propertyValue) ? null : _cachedResult;
            }
        }

        private void EnsureCachedResult()
        {
            if (_cachedResult == null)
            {
                if (_propertyValue == null || _resourceType == null)
                    _cachedResult = (() => _propertyValue);
                else
                {
                    PropertyInfo property = _resourceType.GetProperty(_propertyValue, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (!IsValidProperty(property))
                    {
                        string exceptionMessage = DiagnosticMessages.LocalizableString_LocalizationFailed(_propertyName, _resourceType.FullName, _propertyValue);
                        _cachedResult = delegate
                        {
                            throw new InvalidOperationException(exceptionMessage);
                        };
                    }
                    else
                    {
                        _cachedResult = (() => (string)property.GetValue(null, null));
                    }
                }
            }
        }

        private bool IsValidProperty(PropertyInfo property)
        {
            if (property == null || property.PropertyType != typeof(string))
                return false;

            MethodInfo getMethod = property.GetGetMethod(true);
            if (getMethod == null || !getMethod.IsStatic)
                return false;

            return true;
        }
    }
}
