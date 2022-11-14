using System;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Linq;

namespace WinScreenRec
{
    class MainModel
    {

        ImgProcess m_ImgProcess = new ImgProcess();

        public struct Position
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }
        Position position = new Position();
        ImgProcess.RECT m_RECT = new ImgProcess.RECT();

        public double RectLeft { set; get; }
        public double RectTop { set; get; }
        public double RectHeight { set; get; }
        public double RectWidth { set; get; }
        public string RectangleMargin { set; get; }

        public bool isStartRec { set; get; } = false;
        public bool isStartPrev { set; get; } = true;
        public bool IsMouseDown { set; get; } = false;

        int timerCnt = 0;
        public int GetTimerCnt()
        {
            return timerCnt;
        }

        public Position Getposition()
        {
            return position;
        }

        public ImgProcess.RECT GetRectInfo()
        {
            return m_RECT;
        }

        public delegate void SetRectInformation(double rectHeight, double rectWidth, string rectMargin);
        public delegate void DispCapture(ref System.Drawing.Bitmap bitmap, int minute, int sec);

        public void SetFilePath(string fileName)
        {
            m_ImgProcess.SetFilePath(fileName, m_RECT);
        }


        public void MakePosition(System.Windows.Point pos, SetRectInformation setRectInformation)
        {
            double Xpos = RectLeft;
            double Ypos = RectTop;
            if (IsMouseDown)
            {
                RectHeight = Math.Abs(pos.Y - RectTop);
                RectWidth = Math.Abs(pos.X - RectLeft);

                if ((RectTop > pos.Y) && (RectLeft > pos.X))
                {
                    RectangleMargin = pos.X.ToString() + "," + pos.Y.ToString();
                    Xpos = pos.X;
                    Ypos = pos.Y;
                }
                else if (RectTop > pos.Y)
                {
                    RectangleMargin = RectLeft.ToString() + "," + pos.Y.ToString();
                    Xpos = RectLeft;
                    Ypos = pos.Y;
                }
                else if (RectLeft > pos.X)
                {
                    RectangleMargin = pos.X.ToString() + "," + RectTop.ToString();
                    Xpos = pos.X;
                    Ypos = RectTop;
                }

                var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w is MainWindow);
                var rectCanvas = (Canvas)window.FindName("RectArea");


                position.width = (int)(RectWidth * (SystemParameters.PrimaryScreenWidth / rectCanvas.ActualWidth));
                position.height = (int)(RectHeight * (SystemParameters.PrimaryScreenHeight / rectCanvas.ActualHeight));
                position.top = (int)(Ypos * (SystemParameters.PrimaryScreenHeight / rectCanvas.ActualHeight));
                position.left = (int)(Xpos * (SystemParameters.PrimaryScreenWidth / rectCanvas.ActualWidth));

                setRectInformation(RectHeight, RectWidth, RectangleMargin);
            }
        }


        public void CaptureMovieAsync(DispCapture dispCapture)
        {
            var bitmap = new System.Drawing.Bitmap(
                (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            while (isStartPrev)
            {
                isStartPrev = CaputureScreen(ref bitmap);

                int sec = (GetTimerCnt() / 10) % 60;
                int minute = (GetTimerCnt() / 10) / 60;
                dispCapture(ref bitmap, minute, sec);
            }
            bitmap.Dispose();
        }

        public bool CaputureScreen(ref Bitmap bitmap)
        {
            bool ret = true;

            Position position = Getposition();
            if (position.width <= 0 || position.height <= 0)
            {
                m_RECT.right = (int)SystemParameters.PrimaryScreenWidth;
                m_RECT.bottom = (int)SystemParameters.PrimaryScreenHeight;
                m_RECT.left = 0;
                m_RECT.top = 0;
            }
            else
            {
                m_RECT.right = position.width + position.left;
                m_RECT.bottom = position.height + position.top;
                m_RECT.left = position.left;
                m_RECT.top = position.top;
            }

            m_ImgProcess.GetCaptureImage(isStartRec, m_RECT, ref bitmap);

            if (isStartRec)
            {
                timerCnt++;
            }
            else
            {
                timerCnt = 0;
            }
            if (timerCnt >= 18000)
            {
                ret = false;
                timerCnt = 0;

            }
            return ret;
        }
    }
}
