﻿using DevZest.Data.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class AutoGuidAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Guid);
            }

            [AutoGuid]
            public _Guid Guid { get; private set; }
        }

        private sealed class TestDb : SqlSession
        {
            public TestDb(SqlVersion sqlVersion)
                : base(new SqlConnection())
            {
                SqlVersion = sqlVersion;
            }

            private DbTable<TestModel> _testTable;
            public DbTable<TestModel> TestTable
            {
                get { return GetTable(ref _testTable, nameof(TestTable)); }
            }
        }

        [TestMethod]
        public void AutoGuidAttribute_sql_generation()
        {
            using (var testDb = new TestDb(SqlVersion.Sql11))
            {
                var command = testDb.GetCreateTableCommand(new TestModel(), nameof(TestDb.TestTable), false);
                var expectedSql =
@"CREATE TABLE [TestTable] (
    [Guid] UNIQUEIDENTIFIER NULL DEFAULT(NEWID())
);
";
                Assert.AreEqual(expectedSql, command.CommandText);
            }
        }

        [TestMethod]
        public void AutoGuidAttribute_default_value()
        {
            var dataSet = DataSet<TestModel>.New();
            var dataRow = dataSet.AddRow();
            var dateTime = dataSet._.Guid[dataRow];
            Assert.IsTrue(dateTime.HasValue);
        }
    }
}
