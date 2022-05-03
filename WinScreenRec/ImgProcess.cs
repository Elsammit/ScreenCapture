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
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        VideoWriter writer;
        string RecordFilePath = "";

        public void InitVideoWriter()
        {
            writer = new VideoWriter(RecordFilePath, FourCC.WMV1, 5,
                    new OpenCvSharp.Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
        }

        public void SetFilePath(string filePath)
        {
            RecordFilePath = filePath;
            InitVideoWriter();
        }

        public Bitmap GetCaptureImage(bool isStartRec, int CapWidth, int CapHeight, int LeftPos, int TopPos)
        {
            var screenBmp = new System.Drawing.Bitmap(
                    (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            var bmpGraphics = Graphics.FromImage(screenBmp);
            bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
            WriteVideo(isStartRec, screenBmp, CapWidth, CapHeight, LeftPos, TopPos);

            return screenBmp;
        }

        private void WriteVideo(bool isStartRec, Bitmap screenBmp, int CapWidth, int CapHeight, int LeftPos, int TopPos)
        {
            System.Drawing.Rectangle recta = new System.Drawing.Rectangle(LeftPos, TopPos,
                        CapWidth, CapHeight);
            Bitmap bmp = screenBmp.Clone(recta, screenBmp.PixelFormat);
            Mat mat = BitmapConverter.ToMat(bmp).CvtColor(ColorConversionCodes.RGB2BGR);
            if (isStartRec)
            {
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                Cv2.Resize(mat, mat, new OpenCvSharp.Size((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                writer.Write(mat);
            }
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
