﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DevZest
{
    static partial class EmptyReadOnlyDictionary<TKey, TValue>
    {
        internal static ReadOnlyDictionary<TKey, TValue> Singleton = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());
    }
}
