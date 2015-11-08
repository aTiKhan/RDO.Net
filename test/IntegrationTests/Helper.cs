﻿using DevZest.Data.SqlServer;
using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    internal static class Helper
    {
        internal static IList<int> GetSalesOrderIds(this DbTable<SalesOrder> salesOrders)
        {
            var result = new List<int>();

            using (var reader = ((SqlSession)salesOrders.DbSession).ExecuteReader(salesOrders))
            {
                while (reader.Read())
                {
                    result.Add(salesOrders._.SalesOrderID[reader].Value);
                }
            }
            return result;
        }

        internal static void Verify<T>(this IList<T> list, params T[] expectedValues)
        {
            var expectedCount = expectedValues == null ? 0 : expectedValues.Length;
            Assert.AreEqual(expectedCount, list.Count);
            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(expectedValues[i], list[i]);
        }

        internal static void AddTestDataRows(this DataSet<SalesOrder> salesOrder, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var ordinal = salesOrder.AddRow().Ordinal;

                salesOrder._.DueDate[ordinal] = new DateTime(2015, 9, 30);
                salesOrder._.SalesOrderNumber[ordinal] = "SalesOrderNumber" + (ordinal + 1).ToString();
                salesOrder._.CustomerID[ordinal] = ordinal + 1;
                salesOrder._.ShipMethod[ordinal] = "TRUCK" + (ordinal + 1).ToString();
                salesOrder._.TotalDue[0] = 0;
            }
        }
    }
}