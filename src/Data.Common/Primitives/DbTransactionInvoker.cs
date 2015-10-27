﻿using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DevZest.Data.Primitives
{
    public abstract class DbTransactionInvoker<TConnection, TTransaction> : InterceptableInvoker<IDbTransactionInterceptor<TConnection, TTransaction>>
        where TConnection : DbConnection
        where TTransaction : DbTransaction
    {
        protected DbTransactionInvoker(DbSession dbSession, TConnection connection, IsolationLevel isolationLevel)
            : base(dbSession)
        {
            Check.NotNull(connection, nameof(connection));
            Connection = connection;
            IsolationLevel = isolationLevel;
        }

        public TConnection Connection { get; private set; }

        public IsolationLevel IsolationLevel { get; private set; }

        public TTransaction Transaction { get; private set; }

        internal void Execute(Stack<TTransaction> transactions, Action action)
        {
            InvokeBeginTransaction(transactions);
            try
            {
                action();
                InvokeCommit(transactions);
            }
            catch (Exception)
            {
                InvokeRollback(transactions);
                throw;
            }
        }

        private TTransaction InvokeBeginTransaction(Stack<TTransaction> transactions)
        {
            Invoke(() => { Transaction = BeginTransaction(IsolationLevel); }, x => x.Beginning(this),
                x =>
                {
                    transactions.Push(Transaction);
                    x.Began(this);
                });
            return Transaction;
        }

        private void InvokeCommit(Stack<TTransaction> transactions)
        {
            Invoke(() => Transaction.Commit(), x => x.Committing(this),
                x =>
                {
                    transactions.Pop();
                    x.Committed(this);
                });
        }

        private void InvokeRollback(Stack<TTransaction> transactions)
        {
            Invoke(() => Transaction.Rollback(), x => x.RollingBack(this),
                x =>
                {
                    transactions.Pop();
                    x.RolledBack(this);
                });
        }

        protected abstract TTransaction BeginTransaction(IsolationLevel isolationLevel);
    }
}