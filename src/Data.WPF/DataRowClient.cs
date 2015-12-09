﻿using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public sealed class DataRowClient : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DataRowView();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((DataRowView)element).Manager = (DataRowManager)item;
        }
    }
}
