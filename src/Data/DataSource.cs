﻿namespace DevZest.Data
{
    /// <summary>
    /// Base class of <see cref="DataSet"/>, <see cref="DbQuery{T}"/> and  <see cref="DbTable{T}"/>.
    /// </summary>
    public abstract class DataSource
    {
        /// <summary>
        /// Gets the model associated with this DataSource.
        /// </summary>
        public abstract Model Model { get; }

        /// <summary>
        /// Gets the kind of this DataSource.
        /// </summary>
        public abstract DataSourceKind Kind { get; }

        internal int Revision { get; private set; }

        internal void UpdateRevision()
        {
            Revision++;
        }

        private DataSource _originalDataSource;
        private int? _originalDataSourceRevision;

        private DataSource OriginalDataSource
        {
            get
            {
                var result = _originalDataSource;
                if (result == null || !_originalDataSourceRevision.HasValue)
                    return result;

                return result.Revision != _originalDataSourceRevision.GetValueOrDefault() ? null : result;
            }
        }

        internal void UpdateOriginalDataSource(DataSource originalDataSource, bool revisionInvariant = false)
        {
            if (_originalDataSourceRevision == -1)
                return;

            if (_originalDataSource != null || originalDataSource == null)
            {
                _originalDataSource = null;
                _originalDataSourceRevision = -1;
                return;
            }

            _originalDataSource = originalDataSource;
            if (!revisionInvariant)
                _originalDataSourceRevision = originalDataSource.Revision;
        }

        internal DataSource UltimateOriginalDataSource
        {
            get
            {
                var result = this;
                for (var origin = OriginalDataSource; origin != null; origin = origin.OriginalDataSource)
                    result = origin;
                return result;
            }
        }
    }
}
