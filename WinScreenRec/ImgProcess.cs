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
        VideoWriter writer;

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public void InitVideoWriter()
        {
            writer = new VideoWriter("test.wmv", FourCC.WMV1, 5,
                    new OpenCvSharp.Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
        }

        public Bitmap GetCaptureImage(bool isStartRec)
        {
            var screenBmp = new System.Drawing.Bitmap(
                    (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bmpGraphics = Graphics.FromImage(screenBmp);
            bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);

            Mat mat = BitmapConverter.ToMat(screenBmp).CvtColor(ColorConversionCodes.RGB2BGR);
            if (isStartRec)
            {
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                Cv2.Resize(mat, mat, new OpenCvSharp.Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                writer.Write(mat);
            }
            return screenBmp;
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
