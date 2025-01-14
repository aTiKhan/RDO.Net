﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    /// <summary>
    /// Provides static methods to manipulate <see cref="IValidationErrors"/> object.
    /// </summary>
    public static class ValidationErrors
    {
        private class EmptyGroup : IValidationErrors
        {
            public static EmptyGroup Singleton = new EmptyGroup();
            private EmptyGroup()
            {
            }

            public int Count
            {
                get { return 0; }
            }

            public ValidationError this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public bool IsSealed
            {
                get { return true; }
            }

            public IValidationErrors Add(ValidationError value)
            {
                return value.VerifyNotNull(nameof(value));
            }

            public IValidationErrors Seal()
            {
                return this;
            }

            public IEnumerator<ValidationError> GetEnumerator()
            {
                return EmptyEnumerator<ValidationError>.Singleton;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return EmptyEnumerator<ValidationError>.Singleton;
            }
        }

        private class ListGroup : IValidationErrors
        {
            private bool _isSealed;
            private List<ValidationError> _list = new List<ValidationError>();

            public ListGroup(ValidationError value1, ValidationError value2)
            {
                Debug.Assert(value1 != null && value2 != null);
                Add(value1);
                Add(value2);
            }

            private ListGroup()
            {
            }

            public bool IsSealed
            {
                get { return _isSealed; }
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public ValidationError this[int index]
            {
                get { return _list[index]; }
            }

            public IValidationErrors Seal()
            {
                _isSealed = true;
                return this;
            }

            public IValidationErrors Add(ValidationError value)
            {
                value.VerifyNotNull(nameof(value));

                if (!IsSealed)
                {
                    _list.Add(value);
                    return this;
                }

                if (Count == 0)
                    return value;
                else
                {
                    var result = new ListGroup();
                    for (int i = 0; i < Count; i++)
                        result.Add(this[i]);
                    result.Add(value);
                    return result;
                }
            }

            public IEnumerator<ValidationError> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets an empty <see cref="IValidationErrors"/> object.
        /// </summary>
        public static IValidationErrors Empty
        {
            get { return EmptyGroup.Singleton; }
        }

        internal static IValidationErrors New(ValidationError value1, ValidationError value2)
        {
            Debug.Assert(value1 != null && value2 != null && value1 != value2);
            return new ListGroup(value1, value2);
        }

        /// <summary>
        /// Creates an <see cref="IValidationErrors"/> object from <see cref="ValidationError"/> objects.
        /// </summary>
        /// <param name="values">The <see cref="ValidationError"/> objects.</param>
        /// <returns>The created <see cref="IValidationErrors"/> object.</returns>
        public static IValidationErrors New(params ValidationError[] values)
        {
            values.VerifyNotNull(nameof(values));

            if (values.Length == 0)
                return Empty;

            IValidationErrors result = values.VerifyNotNull(0, nameof(values));
            for (int i = 1; i < values.Length; i++)
                result = result.Add(values.VerifyNotNull(i, nameof(values)));
            return result;
        }
    }
}
