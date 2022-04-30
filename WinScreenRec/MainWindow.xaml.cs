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
using System.Windows.Controls;

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
        bool isDrag = false;
        Thread thread;
        ImgProcess m_ImgProcess = new ImgProcess();
        MousePosition m_MousePosition = new MousePosition();


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

        private void MouseLeftBtnDwn(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Canvas c = sender as Canvas;
            Console.WriteLine("canvas width:{0}, height:{1}", RectArea.Width, RectArea.Height);
            var point = e.GetPosition(c);
            bool buf = m_MousePosition.SetInit(point, RectArea);
            if (buf)
            {
                isDrag = buf;
                c.CaptureMouse();
            }
            
        }

        private void MouseMoving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDrag)
            {
                Console.WriteLine("Mouse Move");
                System.Windows.Point pos = e.GetPosition(RectArea);
                m_MousePosition.DrawRectangle(pos, RectArea);
            }
        }

        private void MouseLeftBtnUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDrag)
            {
                Console.WriteLine("Mouse Left Up");
                Canvas c = sender as Canvas;
                isDrag = false;
                c.ReleaseMouseCapture();
            }
        }
    }
}