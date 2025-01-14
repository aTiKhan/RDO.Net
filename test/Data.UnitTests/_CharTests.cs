﻿using DevZest.Data.Helpers;
using DevZest.Data.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DevZest.Data
{
    [TestClass]
    public class _CharTests
    {
        [TestMethod]
        public void _Char_Param()
        {
            TestParam('A');
            TestParam(null);
        }

        private void TestParam(Char? x)
        {
            _Char column = _Char.Param(x);
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Char_Implicit()
        {
            TestImplicit('A');
            TestImplicit(null);
        }

        private void TestImplicit(Char? x)
        {
            _Char column = x;
            column.VerifyParam(x);
        }

        [TestMethod]
        public void _Char_Const()
        {
            TestConst('A');
            TestConst(null);
        }

        private static void TestConst(char? x)
        {
            _Char column = _Char.Const(x);
            column.VerifyConst(x);
        }

        [TestMethod]
        public void _Char_FromString()
        {
            TestFromString("8", '8');
            TestFromString(null, null);
        }

        private void TestFromString(String x, Char? expectedValue)
        {
            _String column1 = x;
            _Char expr = (_Char)column1;
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(String), typeof(Char?));
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_LessThan()
        {
            TestLessThan('0', '1', true);
            TestLessThan('9', '9', false);
            TestLessThan('9', '8', false);
            TestLessThan('5', null, null);
            TestLessThan(null, '5', null);
            TestLessThan(null, null, null);
        }

        private void TestLessThan(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 < column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_LessThanOrEqual()
        {
            TestLessThanOrEqual('9', '8', false);
            TestLessThanOrEqual('9', '9', true);
            TestLessThanOrEqual('0', '1', true);
            TestLessThanOrEqual('5', null, null);
            TestLessThanOrEqual(null, '5', null);
            TestLessThanOrEqual(null, null, null);
        }

        private void TestLessThanOrEqual(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 <= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.LessThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_GreaterThan()
        {
            TestGreaterThan('1', '0', true);
            TestGreaterThan('1', '1', false);
            TestGreaterThan('0', '1', false);
            TestGreaterThan('5', null, null);
            TestGreaterThan(null, '5', null);
            TestGreaterThan(null, null, null);
        }

        private void TestGreaterThan(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 > column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThan, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_GreaterThanOrEqual()
        {
            TestGreaterThanOrEqual('1', '0', true);
            TestGreaterThanOrEqual('1', '1', true);
            TestGreaterThanOrEqual('0', '1', false);
            TestGreaterThanOrEqual('5', null, null);
            TestGreaterThanOrEqual(null, '5', null);
            TestGreaterThanOrEqual(null, null, null);
        }

        private void TestGreaterThanOrEqual(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 >= column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.GreaterThanOrEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_Equal()
        {
            TestEqual('2', '2', true);
            TestEqual('4', '5', false);
            TestEqual('1', null, null);
            TestEqual(null, null, null);
        }

        private void TestEqual(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 == column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.Equal, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_NotEqual()
        {
            TestNotEqual('1', '1', false);
            TestNotEqual('1', '2', true);
            TestNotEqual('a', null, null);
            TestNotEqual(null, null, null);
        }

        private void TestNotEqual(Char? x, Char? y, bool? expectedValue)
        {
            _Char column1 = x;
            _Char column2 = y;
            var expr = column1 != column2;
            var dbExpr = (DbBinaryExpression)expr.DbExpression;
            dbExpr.Verify(BinaryExpressionKind.NotEqual, column1, column2);
            expr.VerifyEval(expectedValue);
        }

        [TestMethod]
        public void _Char_CastToString()
        {
            TestCastToString(null, null);
            TestCastToString('A', "A");
        }

        private void TestCastToString(Char? x, String expectedValue)
        {
            _Char column1 = x;
            _String expr = column1.CastToString();
            var dbExpr = (DbCastExpression)expr.DbExpression;
            dbExpr.Verify(column1, typeof(Char?), typeof(String));
            expr.VerifyEval(expectedValue);
        }
    }
}
