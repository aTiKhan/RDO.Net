﻿using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data
{
    [TestClass]
    public class JsonViewTests
    {
        [TestMethod]
        public void JsonView_ToJsonString()
        {
            var salesOrderInfo = DataSet<SalesOrderInfo>.ParseJson(Json.SalesOrderInfo_71774);
            var details = salesOrderInfo.GetChild(x => x.SalesOrderDetails);
            var jsonView = salesOrderInfo.Filter(JsonFilter.NoProjection).FilterChildren(details.Filter(JsonFilter.NoProjection));
            Assert.AreEqual(Json.SalesOrder_71774, jsonView.ToJsonString(true));

            jsonView = salesOrderInfo.Filter(JsonFilter.PrimaryKeyOnly);
            var expectedJson =
@"[
   {
      ""SalesOrderID"" : 71774
   }
]";
            Assert.AreEqual(expectedJson, jsonView.ToJsonString(true));

            jsonView = salesOrderInfo.Filter(JsonFilter.Explicit(salesOrderInfo._.SalesOrderID, salesOrderInfo._.RevisionNumber));
            expectedJson =
@"[
   {
      ""SalesOrderID"" : 71774,
      ""RevisionNumber"" : 2
   }
]";
            Assert.AreEqual(expectedJson, jsonView.ToJsonString(true));
        }
    }
}
