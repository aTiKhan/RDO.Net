﻿using System.Data.Common;

namespace DevZest.Data.Addons
{
    internal interface IDbNonQueryInterceptor<TCommand> : IAddon
        where TCommand : DbCommand
    {
        void OnExecuting(TCommand command, AddonInvoker invoker);

        void OnExecuted(TCommand command, int result, AddonInvoker invoker);
    }
}
