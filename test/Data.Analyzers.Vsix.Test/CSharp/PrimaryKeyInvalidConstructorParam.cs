﻿namespace DevZest.Data.Analyzers.Vsix.Test.CSharp
{
    public sealed class PrimaryKeyInvalidConstructorParam : CandidateKey
    {
        public PrimaryKeyInvalidConstructorParam(int id1, _Int32 id2, int id3)
            : base(id2)
        {
        }
    }
}
