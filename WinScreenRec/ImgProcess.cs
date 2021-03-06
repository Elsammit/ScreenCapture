using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;

namespace WinScreenRec
{
    class ImgProcess
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        VideoWriter writer;
        string RecordFilePath = "";
        RECT m_recordData = new RECT();

        public void InitVideoWriter(RECT rect)
        {
            //m_recordData = rect;
            int width = m_recordData.right - m_recordData.left;
            int height = m_recordData.bottom - m_recordData.top;
            writer = new VideoWriter(RecordFilePath, FourCC.WMV1, 5,
                    new OpenCvSharp.Size(width, height));
        }

        public void SetFilePath(string filePath, RECT rect)
        {
            RecordFilePath = filePath;
            InitVideoWriter(rect);
        }

        public Bitmap GetCaptureImage(bool isStartRec, RECT rect)
        {
            var screenBmp = new System.Drawing.Bitmap(
                    (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            var bmpGraphics = Graphics.FromImage(screenBmp);
            bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
            if(!WriteVideo(isStartRec, screenBmp, rect))
            {
                MessageBox.Show("正しくエリアを指定ください", "エリア指定エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return screenBmp;
        }

        private bool WriteVideo(bool isStartRec, Bitmap screenBmp, RECT rect)
        {
            m_recordData = rect;

            int capWidth = m_recordData.right - m_recordData.left;
            int capHeight = m_recordData.bottom - m_recordData.top;
         
            if (capHeight <= 0 || capWidth <= 0)
            {
                Console.WriteLine(" size Error");
                return false;
            }
            System.Drawing.Rectangle rectBuf = new System.Drawing.Rectangle(rect.left, rect.top,
                        capWidth, capHeight);
            Bitmap bmp = screenBmp.Clone(rectBuf, screenBmp.PixelFormat);
            Mat mat = BitmapConverter.ToMat(bmp).CvtColor(ColorConversionCodes.RGB2BGR);
            if (isStartRec)
            {
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                Cv2.Resize(mat, mat, new OpenCvSharp.Size(capWidth, capHeight));
                writer.Write(mat);
            }
            return true;
        }

        private bool GetAppPosition(ref RECT rect)
        {
            bool flag = false;
            string appName = "notepad"; //ここに実行ファイル名(拡張子なし)を記入する.

            try
            {
                var mainWindowHandle = System.Diagnostics.Process.GetProcessesByName(appName)[0].MainWindowHandle;
                flag = GetWindowRect(mainWindowHandle, out rect);
            }
            catch
            {
                flag = false;
            }

            return flag;
        }
    }
}
