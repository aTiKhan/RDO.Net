﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class StringLengthAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text1);
                RegisterColumn((TestModel _) => _.Text2);
            }

            [StringLength(10)]
            public _String Text1 { get; private set; }

            [StringLength(10, MinimumLength = 5)]
            public _String Text2 { get; private set; }
        }

        [TestMethod]
        public void StringLengthAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Text1[row] = "123456";
                    _.Text2[row] = "123456";
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Text1[row] = "12345678901";
                    _.Text2[row] = "123456";
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.StringLengthAttribute, nameof(TestModel.Text1), 10),
                    validationMessages[0].Message);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) =>
                {
                    _.Text1[row] = "123456";
                    _.Text2[row] = "1234";
                });
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.StringLengthAttribute_WithMinLength, nameof(TestModel.Text2), 10, 5),
                    validationMessages[0].Message);
            }
        }
    }
}
