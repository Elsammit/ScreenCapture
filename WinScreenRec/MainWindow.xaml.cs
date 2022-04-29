using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using WinScreenRec;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        bool isStartRec = false;
        bool isStartPrev = true;
        Thread thread;
        ImgProcess m_ImgProcess = new ImgProcess();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (isStartRec)
            {
                isStartRec = false;
                StartButton.Content = "録画開始";
            }
            else
            {
                isStartRec = true;
                StartButton.Content = "録画停止";
            }
        }

        private void CaptureMovieAsync()
        {

            while (isStartPrev)
            {
                var bitmap = m_ImgProcess.GetCaptureImage(isStartRec);
                var hBitmap = bitmap.GetHbitmap();

                Dispatcher.Invoke((Action)(() =>
                {
                    ImgCap.Source = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                }));
                DeleteObject(hBitmap);
                Cv2.WaitKey(90);
            }
        }

        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Close Window");
            isStartPrev = false;
            isStartRec = false;
            thread.Abort();
        }

        private void WindowLoad(object sender, RoutedEventArgs e)
        {
            m_ImgProcess.InitVideoWriter();
            thread = new Thread(new ThreadStart(() =>
            {
                CaptureMovieAsync();
            }));
            thread.Start();
        }
    }
}