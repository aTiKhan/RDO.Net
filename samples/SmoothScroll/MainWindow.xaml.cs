﻿using System.Windows;

namespace SmoothScroll
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            new Presenter().Show(dataView, ListItem.Mock(10000));
        }
    }
}
