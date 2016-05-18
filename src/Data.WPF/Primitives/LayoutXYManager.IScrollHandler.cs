﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    partial class LayoutXYManager
    {
        public ScrollViewer ScrollOwner { get; set; }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        private void InvalidateMeasure()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateMeasure();
        }

        private void InvalidateArrange()
        {
            if (ElementCollection.Parent != null)
                ElementCollection.Parent.InvalidateArrange();
        }

        public abstract double ViewportX { get; }

        public abstract double ViewportY { get; }

        protected double ViewportMain { get; private set; }

        protected double ViewportCross { get; private set; }

        private void RefreshViewport(double valueMain, double valueCross)
        {
            if (ViewportMain.IsClose(valueMain) && ViewportCross.IsClose(valueCross))
                return;
            ViewportMain = valueMain;
            ViewportCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ExtentY { get; }

        public abstract double ExtentX { get; }

        protected double ExtentMain { get; private set; }

        protected double ExtentCross { get; private set; }

        private void RefreshExtent(double valueMain, double valueCross)
        {
            if (ExtentMain.IsClose(valueMain) && ExtentCross.IsClose(valueCross))
                return;
            ExtentMain = valueMain;
            ExtentCross = valueCross;
            InvalidateScrollInfo();
        }

        public abstract double ScrollOffsetX { get; set; }

        public abstract double ScrollOffsetY { get; set; }

        private double _oldScrollOffsetMain;
        private double _scrollOffsetMain;
        protected double ScrollOffsetMain
        {
            get { return _scrollOffsetMain; }
            set { SetScrollOffsetMain(value, true); }
        }

        private void SetScrollOffsetMain(double value, bool invalidateMeasure)
        {
            if (_scrollOffsetMain.IsClose(value))
                return;
            _scrollOffsetMain = value;
            if (invalidateMeasure)
                InvalidateMeasure();
        }

        private double _scrollOffsetCross;
        protected double ScrollOffsetCross
        {
            get { return _scrollOffsetCross; }
            set { SetScrollOffsetCross(value, true); }
        }

        private void SetScrollOffsetCross(double value, bool invalidateArrange)
        {
            if (_scrollOffsetCross.IsClose(value))
                return;
            _scrollOffsetCross = value;
            if (invalidateArrange)
                InvalidateArrange();
        }

        private void RefreshScollOffset(double valueMain, double valueCross)
        {
            bool invalidateScrollInfo = !ScrollOffsetMain.IsClose(valueMain) || !ScrollOffsetCross.IsClose(valueCross);
            _oldScrollOffsetMain = valueMain;
            SetScrollOffsetMain(valueMain, false);
            SetScrollOffsetCross(valueCross, false);
            if (invalidateScrollInfo)
                InvalidateScrollInfo();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }
    }
}
