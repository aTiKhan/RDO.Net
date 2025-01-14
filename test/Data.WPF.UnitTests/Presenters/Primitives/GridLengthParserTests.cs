﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    [TestClass]
    public class GridLengthParserTests
    {
        [TestMethod]
        public void GridLengthParser_Parse()
        {
            {
                var result = GridLengthParser.Parse("10");
                Verify(result, new GridLength(10), 0.0, double.PositiveInfinity);
            }

            {
                var result = GridLengthParser.Parse("10; min: 5");
                Verify(result, new GridLength(10), 5.0, double.PositiveInfinity);
            }

            {
                var result = GridLengthParser.Parse("10; min: 5; max: 20");
                Verify(result, new GridLength(10), 5.0, 20.0);
            }

            {
                var result = GridLengthParser.Parse("10; min: 5; max: 20;");
                Verify(result, new GridLength(10), 5.0, 20.0);
            }

            {
                var result = GridLengthParser.Parse("10; MIN: 5; MAX: 20;");
                Verify(result, new GridLength(10), 5.0, 20.0);
            }

            {
                var result = GridLengthParser.Parse("*; MIN: 5; MAX: 20;");
                Verify(result, new GridLength(1, GridUnitType.Star), 5.0, 20.0);
            }

            {
                var result = GridLengthParser.Parse("2*; MIN: 5; MAX: 20;");
                Verify(result, new GridLength(2, GridUnitType.Star), 5.0, 20.0);
            }

            {
                var result = GridLengthParser.Parse("Auto; MIN: 5; MAX: 20;");
                Verify(result, GridLength.Auto, 5.0, 20.0);
            }
        }

        private static void Verify(GridLengthParser.Result result, GridLength expectedLength, double expectedMinLength, double expectedMaxLength)
        {
            Assert.AreEqual(result.Length, expectedLength);
            Assert.AreEqual(result.MinLength, expectedMinLength);
            Assert.AreEqual(result.MaxLength, expectedMaxLength);
        }

        [TestMethod]
        public void GridLengthParser_Parse_FormatException_expected()
        {
            VerifyFormatExceptionExpected(null);
            VerifyFormatExceptionExpected(string.Empty);
            VerifyFormatExceptionExpected("min: 10");    // missing Length
            VerifyFormatExceptionExpected("10; 10");    // duplicate Length
            VerifyFormatExceptionExpected("10; min: 5; min: 5");    // duplicate min
            VerifyFormatExceptionExpected("10; max: 5; max: 5");    // duplicate max
            VerifyFormatExceptionExpected("10; unknown: 5;");    // unknown name
            VerifyFormatExceptionExpected("10; min: 5;; max: 20;"); // empty pair
            VerifyFormatExceptionExpected("10; min: 5; max: 20;;"); // last empty pair
        }

        private static void VerifyFormatExceptionExpected(string input)
        {
            try
            {
                GridLengthParser.Parse(input);
                Assert.Fail(string.Format("A FormatException should be thrown for input string '{0}'", input));
            }
            catch (FormatException ex)
            {
                Assert.AreEqual(ex.Message, DiagnosticMessages.GridLengthParser_InvalidInput(input));
            }
        }
    }
}
