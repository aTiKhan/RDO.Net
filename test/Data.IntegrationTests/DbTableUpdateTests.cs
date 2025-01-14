﻿using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableUpdateTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public async Task DbTable_UpdateAsync_without_source()
        {
            var log = new StringBuilder();
            using (var db = await MockProductCategory.CreateAsync(CreateDb(log)))
            {
                var count = await db.ProductCategory.Where(x => x.ProductCategoryID > 2).CountAsync();
                Assert.IsTrue(count > 0);
                _DateTime newModifiedDate = new DateTime(2015, 11, 19);
                await db.ProductCategory.UpdateAsync((builder, productCategory) =>
                {
                    builder.Select(newModifiedDate, productCategory.ModifiedDate);
                }, x => x.ProductCategoryID > 2);
                Assert.AreEqual(count, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_Scalar()
        {
            var log = new StringBuilder();
            using (var db = await MockProductCategory.CreateAsync(CreateDb(log)))
            {
                var dataSet = await db.ProductCategory.ToDataSetAsync();
                Assert.IsTrue(dataSet.Count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                dataSet._.ModifiedDate[0] = newModifiedDate;

                await db.ProductCategory.UpdateAsync(dataSet, 0);
                Assert.AreEqual(1, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = await MockProductCategory.CreateAsync(CreateDb(log)))
            {
                var dataSet = await db.ProductCategory.ToDataSetAsync();
                var count = dataSet.Count;
                Assert.IsTrue(count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                for (int i = 0; i < count; i++)
                    dataSet._.ModifiedDate[i] = newModifiedDate;

                await db.ProductCategory.UpdateAsync(dataSet);
                Assert.AreEqual(count, await db.ProductCategory.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_self_increment()
        {
            var log = new StringBuilder();
            using (var db = await MockSalesOrder.CreateAsync(CreateDb(log)))
            {
                _Int32 salesOrderId = 1;
                var dataSet = await db.SalesOrderHeader.Where(_ => _.SalesOrderID == salesOrderId).ToDataSetAsync();
                Assert.AreEqual(1, dataSet.Count);
                var revisionNumber = dataSet._.RevisionNumber[0];

                await db.SalesOrderHeader.UpdateAsync((m, _) => m.Select(_.RevisionNumber + 1, _.RevisionNumber), _ => _.SalesOrderID == salesOrderId);
                dataSet = await db.SalesOrderHeader.Where(_ => _.SalesOrderID == salesOrderId).ToDataSetAsync();
                Assert.AreEqual(1, dataSet.Count);
                Assert.AreEqual(revisionNumber + 1, dataSet._.RevisionNumber[0]);
            }
        }

    }
}
