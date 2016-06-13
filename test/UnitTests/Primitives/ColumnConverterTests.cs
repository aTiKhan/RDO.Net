﻿using DevZest.Data.Resources;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class ColumnConverterTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ColumnConverter.EnsureInitialized(typeof(_Int32));
        }

        [TestMethod]
        public void Converter_Column()
        {
            var salesOrder = new SalesOrder();
            var json = salesOrder.SalesOrderID.ToJson(true);
            Assert.AreEqual(Json.Converter_Column, json);

            var columnFromJson = Column.FromJson(salesOrder, json);
            Assert.AreEqual(salesOrder.SalesOrderID, columnFromJson);
        }

        [TestMethod]
        public void Converter_ConstantExpression()
        {
            _Int32 column = _Int32.Const(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ConstantExpression, json);

            var columnFromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_ParamExpression_NullSourceColumn()
        {
            _Int32 column = _Int32.Param(5);
            var json = column.ToJson(true);

            Assert.AreEqual(Json.Converter_ParamExpression_NullSourceColumn, json);

            var columnFromJson = (_Int32)Column.FromJson(null, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }

        [TestMethod]
        public void Converter_ParamExpression_NotNullSourceColumn()
        {
            var salesOrder = new SalesOrder();
            var column = _Int32.Param(5, salesOrder.SalesOrderID);
            var json = column.ToJson(true);
            Assert.AreEqual(Json.Converter_ParamExpression_NotNullSourceColumn, json);

            var columnFromJson = (_Int32)Column.FromJson(salesOrder, json);
            Assert.AreEqual(5, columnFromJson.Eval());
        }
    }
}
