﻿using System.Windows;

namespace DevZest.Data.Windows
{
    public abstract class ScalarGridItem : GridItem
    {
        private FlowMode _flowMode;
        public FlowMode FlowMode
        {
            get { return _flowMode; }
            set
            {
                VerifyNotSealed();
                _flowMode = value;
            }
        }
    }
}
