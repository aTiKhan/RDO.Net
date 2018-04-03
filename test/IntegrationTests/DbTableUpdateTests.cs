﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DevZest.Data
{
    [TestClass]
    public class DbTableUpdateTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void DbTable_Update_without_source()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var count = db.ProductCategories.Where(x => x.ProductCategoryID > 2).Count();
                Assert.IsTrue(count > 0);
                _DateTime newModifiedDate = new DateTime(2015, 11, 19);
                db.ProductCategories.Update((builder, productCategory) =>
                {
                    builder.Map(newModifiedDate, productCategory.ModifiedDate);
                }, x => x.ProductCategoryID > 2).Execute();
                Assert.AreEqual(count, db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).Count());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_without_source()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var count = await db.ProductCategories.Where(x => x.ProductCategoryID > 2).CountAsync();
                Assert.IsTrue(count > 0);
                _DateTime newModifiedDate = new DateTime(2015, 11, 19);
                await db.ProductCategories.Update((builder, productCategory) =>
                {
                    builder.Map(newModifiedDate, productCategory.ModifiedDate);
                }, x => x.ProductCategoryID > 2).ExecuteAsync();
                Assert.AreEqual(count, await db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_Scalar()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var dataSet = db.ProductCategories.ToDataSet();
                Assert.IsTrue(dataSet.Count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                dataSet._.ModifiedDate[0] = newModifiedDate;

                db.ProductCategories.Update(dataSet, 0).Execute();
                Assert.AreEqual(1, db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).Count());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_Scalar()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var dataSet = await db.ProductCategories.ToDataSetAsync();
                Assert.IsTrue(dataSet.Count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                dataSet._.ModifiedDate[0] = newModifiedDate;

                await db.ProductCategories.Update(dataSet, 0).ExecuteAsync();
                Assert.AreEqual(1, await db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }

        [TestMethod]
        public void DbTable_Update_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var dataSet = db.ProductCategories.ToDataSet();
                var count = dataSet.Count;
                Assert.IsTrue(count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                for (int i = 0; i < count; i++)
                    dataSet._.ModifiedDate[i] = newModifiedDate;

                db.ProductCategories.Update(dataSet).Execute();
                Assert.AreEqual(count, db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).Count());
            }
        }

        [TestMethod]
        public async Task DbTable_UpdateAsync_from_DataSet()
        {
            var log = new StringBuilder();
            using (var db = new ProductCategoryMockDb().Initialize(OpenDb(log)))
            {
                var dataSet = await db.ProductCategories.ToDataSetAsync();
                var count = dataSet.Count;
                Assert.IsTrue(count > 1);
                var newModifiedDate = new DateTime(2015, 11, 19);
                for (int i = 0; i < count; i++)
                    dataSet._.ModifiedDate[i] = newModifiedDate;

                await db.ProductCategories.Update(dataSet).ExecuteAsync();
                Assert.AreEqual(count, await db.ProductCategories.Where(x => x.ModifiedDate == newModifiedDate).CountAsync());
            }
        }
    }
}
