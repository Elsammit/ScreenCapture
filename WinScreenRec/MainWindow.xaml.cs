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
using Microsoft.Win32;

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
        private UIElement canvasStock = new UIElement();
        WinScreenRec.MousePosition.Position position =
                    new WinScreenRec.MousePosition.Position();


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

                var dialog = new SaveFileDialog();
                dialog.Title = "ファイルを保存";
                dialog.Filter = "動画ファイル|*.wmv";
                // ダイアログを表示
                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show(dialog.FileName);

                    m_ImgProcess.SetFilePath(dialog.FileName);
                }

                isStartRec = true;
                StartButton.Content = "録画停止";
            }
        }

        private void CaptureMovieAsync()
        {

            while (isStartPrev)
            {
                int capWidth = 0;
                int capHeight = 0;
                int leftPos = 0;
                int topPos = 0;

                if(position.width <=0 || position.height <= 0)
                {
                    capWidth = (int)SystemParameters.PrimaryScreenWidth;
                    capHeight = (int)SystemParameters.PrimaryScreenHeight;
                }
                else {
                    capWidth = position.width;
                    capHeight = position.height;
                    leftPos = position.left;
                    topPos = position.top;
                }
                var bitmap = m_ImgProcess.GetCaptureImage(isStartRec, capWidth, capHeight, leftPos, topPos);
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
            thread = new Thread(new ThreadStart(() =>
            {
                CaptureMovieAsync();
            }));
            thread.Start();
        }

        private void MouseLeftBtnDwn(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isStartRec)
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
            
        }

        private void MouseMoving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDrag)
            {
                System.Windows.Point pos = e.GetPosition(RectArea);

                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();

                RectArea.Children.Remove(canvasStock);

                m_MousePosition.DrawRectangle(pos, RectArea.Width, RectArea.Height,
                    ref position, ref rectangle);

                RectArea.Children.Add(rectangle);
                canvasStock = rectangle;
            }
        }

        private void MouseLeftBtnUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isDrag)
            {
                Canvas c = sender as Canvas;
                isDrag = false;
                c.ReleaseMouseCapture();
            }
        }
    }
}