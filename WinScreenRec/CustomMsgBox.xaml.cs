using System.Windows;

namespace WinScreenRec
{
    /// <summary>
    /// CustomMsgBox.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomMsgBox : Window
    {
        public CustomMsgBox()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
