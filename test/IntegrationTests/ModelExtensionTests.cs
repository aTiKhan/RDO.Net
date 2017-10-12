﻿using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class ModelExtensionTests : AdventureWorksTestsBase
    {
        [TestMethod]
        public void ModelExtension_sales_order_detail()
        {
            using (var db = OpenDb())
            {
                var salesOrderDetails = db.CreateQuery(_ => _.SetExtension<SalesOrderDetail.Ext>(),
                    (DbQueryBuilder builder, SalesOrderDetail _) =>
                    {
                        SalesOrderDetail d;
                        Product p;
                        builder.From(db.SalesOrderDetails, out d)
                            .InnerJoin(db.Products, d.Product, out p)
                            .AutoSelect()
                            .Where(d.SalesOrderID == _Int32.Const(71774));
                    });

                var json = salesOrderDetails.ToDataSet().ToJsonString(true);
                var expectedJson = Strings.ExpectedJSON_SalesOrderDetail_71774_with_ext.Trim();
                Assert.AreEqual(expectedJson, json);

                var dataSet = DataSet<SalesOrderDetail>.ParseJson(json, _ => _.SetExtension<SalesOrderDetail.Ext>());
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }

        [TestMethod]
        public void ModelExtension_sales_order()
        {
            using (var db = OpenDb())
            {
                var salesOrders = db.CreateQuery(_ => _.SetExtension<SalesOrder.Ext>(),
                    (DbQueryBuilder builder, SalesOrder _) =>
                    {
                        var ext = _.GetExtension<SalesOrder.Ext>();
                        SalesOrder o;
                        Customer c;
                        Address shipTo, billTo;
                        builder.From(db.SalesOrders, out o)
                            .InnerJoin(db.Customers, o.Customer, out c)
                            .InnerJoin(db.Addresses, o.ShipToAddress, out shipTo)
                            .InnerJoin(db.Addresses, o.BillToAddress, out billTo)
                            .AutoSelect()
                            .AutoSelect(shipTo, ext.ShipToAddress)
                            .AutoSelect(billTo, ext.BillToAddress)
                            .Where(o.SalesOrderID == _Int32.Const(71774));
                    });

                var json = salesOrders.ToDataSet().ToJsonString(true);
                var expectedJson = Strings.ExpectedJSON_SalesOrder_71774_with_ext;
                Assert.AreEqual(expectedJson, json);

                var dataSet = DataSet<SalesOrder>.ParseJson(json, _ => _.SetExtension<SalesOrder.Ext>());
                Assert.AreEqual(expectedJson, dataSet.ToJsonString(true));
            }
        }
    }
}
