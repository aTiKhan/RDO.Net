﻿using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingModelAttribute : Model
    {
        [_CheckConstraint]
        private _Boolean CK_AlwaysTrue
        {
            get { return _Boolean.Const(true); }
        }
    }
}
