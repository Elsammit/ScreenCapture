using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WinScreenRec
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "ApplicationName");

        private void AppStartup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("ApplicationName は既に起動しています。", "二重起動防止", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                mutex.Close();
                mutex = null;
                this.Shutdown();
                //return;
            }
            else
            {
                Console.WriteLine("Start Apllication");
            }
        }

        private void AppExit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                Console.WriteLine("Close Application Release mutex");
                mutex.ReleaseMutex();
                mutex.Close();
            }
            else
            {
                Console.WriteLine("Close Application");
            }
        }
    }


}
