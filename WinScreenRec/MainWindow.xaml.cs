using System.Windows;

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