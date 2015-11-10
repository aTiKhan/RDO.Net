﻿using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbSessionTests : AdventureWorksTestsBase
    {
        private static DbQuery<SalesOrder> CreateSalesOrdersQuery(Db db)
        {
            return db.CreateQuery((DbQueryBuilder2 builder, SalesOrder model) =>
            {
                SalesOrder h;
                builder.From(db.SalesOrders, out h)
                    .AutoSelect()
                    .Where(h.SalesOrderID == _Int32.Const(71774) | h.SalesOrderID == _Int32.Const(71776))
                    .OrderBy(h.SalesOrderID);
            });
        }

        private static void GetSalesOrderDetails(Db db, DbQueryBuilder2 queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetails, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public void DbSession_CreateQuery()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbQueryBuilder2 builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, salesOrders.GetInitialRowCount());
                Assert.AreEqual(3, salesOrderDetails.GetInitialRowCount());
            }
            var expectedSql =
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_CreateQuery_async_child()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = await salesOrders.CreateChildAsync(x => x.SalesOrderDetails,
                    (DbQueryBuilder2 builder, SalesOrderDetail model) => GetSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, await salesOrders.GetInitialRowCountAsync());
                Assert.AreEqual(3, await salesOrderDetails.GetInitialRowCountAsync());
            }
            var expectedSql =
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        private static void GetDistinctSalesOrderDetails(Db db, DbAggregateQueryBuilder2 queryBuilder, SalesOrderDetail model)
        {
            SalesOrderDetail d;
            queryBuilder.From(db.SalesOrderDetails, out d)
                .AutoSelect()
                .OrderBy(d.SalesOrderDetailID);
        }

        [TestMethod]
        public void DbSession_CreateQuery_aggregate_child()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var salesOrders = CreateSalesOrdersQuery(db);
                var salesOrderDetails = salesOrders.CreateChild(x => x.SalesOrderDetails, (DbAggregateQueryBuilder2 builder, SalesOrderDetail model) => GetDistinctSalesOrderDetails(db, builder, model));

                Assert.AreEqual(2, salesOrders.GetInitialRowCount());
                Assert.AreEqual(3, salesOrderDetails.GetInitialRowCount());
            }
            var expectedSql =
@"CREATE TABLE [#sys_sequential_SalesOrder] (
    [SalesOrderID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrder]
([SalesOrderID])
SELECT [SalesOrder].[SalesOrderID] AS [SalesOrderID]
FROM [SalesLT].[SalesOrderHeader] [SalesOrder]
WHERE (([SalesOrder].[SalesOrderID] = 71774) OR ([SalesOrder].[SalesOrderID] = 71776))
ORDER BY [SalesOrder].[SalesOrderID];

CREATE TABLE [#sys_sequential_SalesOrderDetail] (
    [SalesOrderID] INT NOT NULL,
    [SalesOrderDetailID] INT NOT NULL,
    [sys_row_id] INT NOT NULL IDENTITY(1, 1)

    PRIMARY KEY NONCLUSTERED ([SalesOrderID], [SalesOrderDetailID]),
    UNIQUE CLUSTERED ([sys_row_id] ASC)
);

INSERT INTO [#sys_sequential_SalesOrderDetail]
([SalesOrderID], [SalesOrderDetailID])
SELECT
    [SalesOrderDetail].[SalesOrderID] AS [SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID] AS [SalesOrderDetailID]
FROM
    ([SalesLT].[SalesOrderDetail] [SalesOrderDetail]
    INNER JOIN
    [#sys_sequential_SalesOrder] [sys_sequential_SalesOrder]
    ON [SalesOrderDetail].[SalesOrderID] = [sys_sequential_SalesOrder].[SalesOrderID])
GROUP BY
    [SalesOrderDetail].[SalesOrderID],
    [SalesOrderDetail].[SalesOrderDetailID],
    [SalesOrderDetail].[OrderQty],
    [SalesOrderDetail].[ProductID],
    [SalesOrderDetail].[UnitPrice],
    [SalesOrderDetail].[UnitPriceDiscount],
    [SalesOrderDetail].[LineTotal],
    [SalesOrderDetail].[RowGuid],
    [SalesOrderDetail].[ModifiedDate],
    [sys_sequential_SalesOrder].[sys_row_id]
ORDER BY [sys_sequential_SalesOrder].[sys_row_id] ASC, [SalesOrderDetail].[SalesOrderDetailID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public void DbSession_ExecuteReader()
        {
            var log = new StringBuilder();
            using (var db = OpenDb(log))
            {
                var customers = db.Customers.OrderBy(x => x.CustomerID);
                var c = customers._;
                using (var reader = db.ExecuteReader(customers))
                {
                    reader.Read();
                    var id = c.CustomerID[reader];
                    Assert.AreEqual(1, id);
                }
            }
            var expectedSql =
@"SELECT
    [Customer].[CustomerID] AS [CustomerID],
    [Customer].[NameStyle] AS [NameStyle],
    [Customer].[Title] AS [Title],
    [Customer].[FirstName] AS [FirstName],
    [Customer].[MiddleName] AS [MiddleName],
    [Customer].[LastName] AS [LastName],
    [Customer].[Suffix] AS [Suffix],
    [Customer].[CompanyName] AS [CompanyName],
    [Customer].[SalesPerson] AS [SalesPerson],
    [Customer].[EmailAddress] AS [EmailAddress],
    [Customer].[Phone] AS [Phone],
    [Customer].[PasswordHash] AS [PasswordHash],
    [Customer].[PasswordSalt] AS [PasswordSalt],
    [Customer].[RowGuid] AS [RowGuid],
    [Customer].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[Customer] [Customer]
ORDER BY [Customer].[CustomerID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }

        [TestMethod]
        public async Task DbSession_ExecuteReaderAsync()
        {
            var log = new StringBuilder();
            using (var db = await OpenDbAsync(log))
            {
                var customers = db.Customers.OrderBy(x => x.CustomerID);
                var c = customers._;
                using (var reader = await db.ExecuteReaderAsync(customers))
                {
                    await reader.ReadAsync();
                    var id = c.CustomerID[reader];
                    Assert.AreEqual(1, id);
                }
            }
            var expectedSql =
@"SELECT
    [Customer].[CustomerID] AS [CustomerID],
    [Customer].[NameStyle] AS [NameStyle],
    [Customer].[Title] AS [Title],
    [Customer].[FirstName] AS [FirstName],
    [Customer].[MiddleName] AS [MiddleName],
    [Customer].[LastName] AS [LastName],
    [Customer].[Suffix] AS [Suffix],
    [Customer].[CompanyName] AS [CompanyName],
    [Customer].[SalesPerson] AS [SalesPerson],
    [Customer].[EmailAddress] AS [EmailAddress],
    [Customer].[Phone] AS [Phone],
    [Customer].[PasswordHash] AS [PasswordHash],
    [Customer].[PasswordSalt] AS [PasswordSalt],
    [Customer].[RowGuid] AS [RowGuid],
    [Customer].[ModifiedDate] AS [ModifiedDate]
FROM [SalesLT].[Customer] [Customer]
ORDER BY [Customer].[CustomerID];
";
            Assert.AreEqual(expectedSql.Trim(), log.ToString().Trim());
        }
    }
}
