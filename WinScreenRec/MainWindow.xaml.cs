using System;
using System.Windows;
//using System.Windows.Media.Imaging;
//using System.Drawing;
//using System.Windows.Interop;
using System.Threading;
//using OpenCvSharp;
//using OpenCvSharp.Extensions;
//using System.Runtime.InteropServices;
//using WinScreenRec;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WinScreenRec
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();

            this.DataContext = new MainViewModel();
        }
    }
}