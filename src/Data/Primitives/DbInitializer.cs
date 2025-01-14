﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Generic base class to initialize the database.
    /// </summary>
    /// <typeparam name="T">Type of database session.</typeparam>
    public abstract class DbInitializer<T> : DbInitializer
        where T : DbSession
    {
        /// <summary>
        /// Gets the prototype database session.
        /// </summary>
        public new T Db
        {
            get { return (T)base.Db; }
        }

        /// <summary>
        /// Creates the database.
        /// </summary>
        /// <param name="db">The prototype database session object.</param>
        /// <param name="progress">Provider for progress update.</param>
        /// <param name="ct">The async cancellation token.</param>
        /// <returns>The database session for newly created database</returns>
        public abstract Task<T> GenerateAsync(T db, IProgress<DbInitProgress> progress = null, CancellationToken ct = default(CancellationToken));
    }

    /// <summary>
    /// Base class to initialize the database.
    /// </summary>
    public abstract class DbInitializer : IProgress<DbInitProgress>
    {
        internal async Task InitializeAsync(DbSession db, string paramName, IProgress<DbInitProgress> progress, CancellationToken ct)
        {
            Verify(db, paramName);
            Db = db;
            db.Generator = this;

            _isInitializing = true;
            await OnInitializingAsync(ct);
            Initialize();
            RemoveDependencyTables();
            RemoveDependencyForeignKeys();
            await CreateTablesAsync(progress, ct);
            _pendingTables.Clear();
            _isInitializing = false;
        }

        internal virtual Task OnInitializingAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private void Verify(DbSession db, string paramName)
        {
            db.VerifyNotNull(paramName);
            db.VerifyNoGenerator();
            if (Db != null)
                throw new InvalidOperationException(DiagnosticMessages.DbGenerator_InitializeTwice);
        }

        /// <summary>
        /// Gets the prototype database session.
        /// </summary>
        public DbSession Db { get; private set; }

        private void RemoveDependencyTables()
        {
            var toRemove = new List<IDbTable>();
            foreach (var table in _tables)
            {
                if (!_pendingTables.ContainsKey(table))
                    toRemove.Add(table);
            }

            foreach (var table in toRemove)
                _tables.Remove(table);
        }

        private void RemoveDependencyForeignKeys()
        {
            var tableNames = new HashSet<string>();
            foreach (var table in _tables)
                tableNames.Add(table.Name);

            foreach (var table in _tables)
            {
                var model = table.Model;
                var fkConstraints = model.GetAddons<DbForeignKeyConstraint>();
                foreach (var fkConstraint in fkConstraints)
                {
                    if (!tableNames.Contains(fkConstraint.ReferencedTableName))
                        model.BrutalRemoveAddon(((IAddon)fkConstraint).Key);
                }
            }
        }

        private async Task CreateTablesAsync(IProgress<DbInitProgress> progress, CancellationToken ct)
        {
            for (int i = 0; i < _tables.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var table = _tables[i];
                if (progress != null)
                    progress.Report(new DbInitProgress(table, i, _tables.Count));
                await Db.CreateTableAsync(table.Model, false, ct);
                var action = _pendingTables[table];
                if (action != null)
                    await action.Invoke(ct);
            }
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        protected abstract void Initialize();

        private Dictionary<IDbTable, Func<CancellationToken, Task>> _pendingTables = new Dictionary<IDbTable, Func<CancellationToken, Task>>();

        internal void AddTable(IDbTable dbTable, Func<CancellationToken, Task> action)
        {
            dbTable.VerifyNotNull(nameof(dbTable));
            if (dbTable.DbSession != Db)
                throw new ArgumentException(DiagnosticMessages.DbGenerator_InvalidTable, nameof(dbTable));
            if (!_isInitializing)
                throw new InvalidOperationException(DiagnosticMessages.DbGenerator_AddTableOnlyAllowedDuringInitialization);
            if (_pendingTables.ContainsKey(dbTable))
                throw new ArgumentException(DiagnosticMessages.DbGenerator_DuplicateTable(dbTable.Name), nameof(dbTable));

            _pendingTables.Add(dbTable, action);
        }

        private bool _isInitializing;
        private HashSet<string> _creatingTableNames = new HashSet<string>();

        private sealed class TableCollection : KeyedCollection<string, IDbTable>
        {
            protected override string GetKeyForItem(IDbTable item)
            {
                return item.Identifier;
            }
        }

        private TableCollection _tables = new TableCollection();

        internal DbTable<T> GetTable<T>(string propertyName)
            where T : Model, new()
        {
            if (_tables.Contains(propertyName))
            {
                var result = _tables[propertyName];
                var resultModelType = result.GetType().GenericTypeArguments[0];
                if (typeof(T) != resultModelType)
                    throw new InvalidOperationException(DiagnosticMessages.DbGenerator_ModelTypeMismatch(typeof(T).FullName, resultModelType.FullName, propertyName));
                return (DbTable<T>)result;
            }

            if (!_isInitializing)
                return null;

            if (_creatingTableNames.Contains(propertyName))
                throw new InvalidOperationException(DiagnosticMessages.DbGenerator_CircularReference(propertyName));

            _creatingTableNames.Add(propertyName);

            var table = DbTable<T>.Create(new T(), Db, propertyName, Initialize);
            _tables.Add(table);
            _creatingTableNames.Remove(propertyName);
            return table;
        }

        private void Initialize<T>(DbTable<T> dbTable)
            where T : Model, new()
        {
            DbTablePropertyAttribute.WireupAttributes(dbTable);
            dbTable.Name = GetTableName(dbTable.Name);
        }

        internal virtual string GetTableName(string name)
        {
            return name;
        }

        /// <summary>
        /// Reports the progress.
        /// </summary>
        /// <param name="progress">The database initialization progress.</param>
        protected virtual void Report(DbInitProgress progress)
        {
            Console.WriteLine(UserMessages.DbGenerator_ReportProgress(progress.DbTable.Name));
        }

        void IProgress<DbInitProgress>.Report(DbInitProgress value)
        {
            Report(value);
        }
    }
}
