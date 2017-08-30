﻿namespace DevZest.Data
{
    /// <summary></summary>
    /// <remarks>All members of <see cref="IValidator"/> are serializable.</remarks>
    public interface IValidator
    {
        string MessageId { get; }
        ValidationSeverity Severity { get; }
        IColumns Columns { get; }
        _Boolean ValidCondition { get; }
        _String Message { get; }
    }
}
