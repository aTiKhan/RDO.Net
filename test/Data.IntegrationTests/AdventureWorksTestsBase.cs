﻿using DevZest.Samples.AdventureWorksLT;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    public abstract class AdventureWorksTestsBase
    {
        protected Db CreateDb()
        {
            return new Db(App.GetConnectionString());
        }

        protected Db CreateDb(StringBuilder logger, LogCategory logCategory = LogCategory.CommandText)
        {
            return new Db(App.GetConnectionString(), db =>
            {
                db.SetLogger(s => logger.Append(s), logCategory);
            });
        }

        protected async Task<DataSet<SalesOrderInfo>> GetSalesOrderInfoAsync(int salesOrderID)
        {
            using (var db = CreateDb())
            {
                return await db.GetSalesOrderInfoAsync(salesOrderID);
            }
        }
    }
}
