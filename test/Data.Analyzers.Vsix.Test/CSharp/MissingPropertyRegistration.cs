﻿using DevZest.Data.Annotations;

namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public class MissingPropertyRegistration : Model
    {
        public _String Column { get; private set; }

        public int NotAColumn { get; private set; }
    }
}
