﻿using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Annotations
{
    /// <summary>Defines name of the column in the database.</summary>
    public sealed class DbColumnAttribute : ColumnAttribute
    {
        public DbColumnAttribute()
        {
        }

        /// <summary>Initializes a new instance of <see cref="DbColumnAttribute"/> object.</summary>
        /// <param name="dbColumnName">The database column name.</param>
        public DbColumnAttribute(string dbColumnName)
        {
            Name = dbColumnName;
        }

        /// <summary>Gets or sets the column name for the column.</summary>
        public string Name { get; private set; }

        public string Description { get; set; }

        /// <inheritdoc/>
        protected sealed override void Initialize(Column column)
        {
            column.DbColumnName = Name;
            column.DbColumnDescription = Description;
        }
    }
}