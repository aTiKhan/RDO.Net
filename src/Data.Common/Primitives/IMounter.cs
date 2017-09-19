﻿using System;

namespace DevZest.Data.Primitives
{
    internal interface IMounter<TTarget, TProperty>
    {
        Type OwnerType { get; }
        string Name { get; }
        Type ParentType { get; }
        TProperty GetInstance(TTarget target);
        TProperty Mount(TTarget target);
    }
}