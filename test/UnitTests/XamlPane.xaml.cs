﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DevZest.Data.Windows
{
    /// <summary>
    /// Interaction logic for XamlPane.xaml
    /// </summary>
    public partial class XamlPane : Pane
    {
        public const string NAME_LEFT = "LEFT";
        public const string NAME_RIGHT = "LEFT";

        public XamlPane()
        {
            InitializeComponent();
        }
    }
}
