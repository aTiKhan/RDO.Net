﻿using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class RowPresenterTests : RowManagerTestsBase
    {
        [TestMethod]
        public void RowPresenter_GetEditValue()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("Name-1-1", rows[1].GetValue(dataSet._.Name));

            rowManager.CurrentRow = rows[0];
            rows[0].EditValue(dataSet._.Name, "NewName-1");
            rowManager.CommitEdit();
            rowManager.CurrentRow = rows[1];
            rows[1].EditValue(dataSet._.Name, "NewName-1-1");
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0].GetValue(dataSet._.Name));
            Assert.AreEqual("NewName-1-1", rows[1].GetValue(dataSet._.Name));
        }

        [TestMethod]
        public void RowPresenter_GetEditValue_Object()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();

            Assert.AreEqual("Name-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("Name-1-1", rows[1][dataSet._.Name]);

            rows[0][dataSet._.Name] = "NewName-1";
            rowManager.CommitEdit();
            rows[1][dataSet._.Name] = "NewName-1-1";
            rowManager.CommitEdit();
            Assert.AreEqual("NewName-1", rows[0][dataSet._.Name]);
            Assert.AreEqual("NewName-1-1", rows[1][dataSet._.Name]);
        }

        [TestMethod]
        public void RowPresenter_CancelEdit()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, RowPlaceholderPosition.Bottom);

            var row = rowManager.Rows[0];
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));

            rowManager.CurrentRow = rowManager.Rows[0];
            row.EditValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            rowManager.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.AreEqual(SalesOrderStatus.Shipped, row.GetValue(dataSet._.Status));
        }

        [TestMethod]
        public void RowPresenter_CancelEdit_Eof()
        {
            var dataSet = DataSet<SalesOrder>.New();
            var rowManager = CreateRowManager(dataSet, RowPlaceholderPosition.Bottom);

            var rows = rowManager.Rows;
            var row = rows[0];
            Assert.AreEqual(1, rows.Count);
            Assert.IsTrue(row.IsPlaceholder);

            rowManager.CurrentRow = row;
            row.EditValue(dataSet._.Status, SalesOrderStatus.InProcess);
            Assert.IsTrue(row.IsPlaceholder);
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(SalesOrderStatus.InProcess, row.GetValue(dataSet._.Status));
            Assert.IsTrue(row.IsEditing);
            rowManager.CancelEdit();
            Assert.IsFalse(row.IsEditing);
            Assert.IsTrue(row.IsPlaceholder);
            Assert.AreEqual(1, rows.Count);
        }

        [TestMethod]
        public void RowPresenter_Delete()
        {
            var dataSet = DataSet<SalesOrder>.ParseJson(StringRes.Sales_Order_71774);
            var rowManager = CreateRowManager(dataSet, RowPlaceholderPosition.None);
            var rows = rowManager.Rows;
            Assert.AreEqual(1, rows.Count);
            rows[0].Delete();
            Assert.AreEqual(0, rows.Count);
        }

        [TestMethod]
        public void RowPresenter_Delete_Hierarchical()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            VerifyIndex(rows);
            VerifyDepths(rows, 0, 0, 0);
            rows[0].Delete();
            VerifyIndex(rows);
            VerifyDepths(rows, 0, 0);
        }

        [TestMethod]
        public void RowPresenter_InsertChildRow()
        {
            var dataSet = MockProductCategories(3);
            var rowManager = CreateRowManager(dataSet);
            var rows = rowManager.Rows;
            rows[0].Expand();
            VerifyDepths(rows, 0, 1, 1, 1, 0, 0);

            //var newChildRow = rows[0].InsertChildRow(1);
            VerifyDepths(rows, 0, 1, 1, 1, 1, 0, 0);
        }
    }
}
