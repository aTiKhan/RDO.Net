﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace DevZest.Data.Annotations
{
    [TestClass]
    public class CreditCardAttributeTests
    {
        private sealed class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.CreditCardNumber);
            }

            [CreditCard]
            public _String CreditCardNumber { get; private set; }
        }

        [TestMethod]
        public void CreditCardAttribute()
        {
            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) => _.CreditCardNumber[row] = "4392 2500 0980 2983");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(0, validationMessages.Count);
            }

            {
                var dataSet = DataSet<TestModel>.Create();
                var dataRow = dataSet.AddRow((_, row) => _.CreditCardNumber[row] = "4392 2500 0980 2980");
                var validationMessages = dataSet._.Validate(dataRow);
                Assert.AreEqual(1, validationMessages.Count);
                Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, UserMessages.CreditCardAttribute, nameof(TestModel.CreditCardNumber)),
                    validationMessages[0].Message);
            }
        }

    }
}
