using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Linq;

namespace WinScreenRec
{
    class MainModel
    {
        public struct Position
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }
        Position position = new Position();

        public double RectLeft { set; get; }
        public double RectTop { set; get; }
        public double RectHeight { set; get; }
        public double RectWidth { set; get; }
        public string RectangleMargin { set; get; }

        public Position Getposition()
        {
            return position;
        }

        public void MakePosition(System.Windows.Point pos, bool isMouseDown)
        {
            double Xpos = RectLeft;
            double Ypos = RectTop;
            if (isMouseDown)
            {
                RectHeight = Math.Abs(pos.Y - RectTop);
                RectWidth = Math.Abs(pos.X - RectLeft);

                if ((RectTop > pos.Y) && (RectLeft > pos.X))
                {
                    RectangleMargin = pos.X.ToString() + "," + pos.Y.ToString();
                    Xpos = RectLeft;
                    Ypos = RectTop;
                }
                else if (RectTop > pos.Y)
                {
                    RectangleMargin = RectLeft.ToString() + "," + pos.Y.ToString();
                    Xpos = pos.X;
                    Ypos = RectTop;
                }
                else if (RectLeft > pos.X)
                {
                    RectangleMargin = pos.X.ToString() + "," + RectTop.ToString();
                    Xpos = RectLeft;
                    Ypos = pos.Y;
                }

                var window = Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w is MainWindow);
                var test = (Canvas)window.FindName("RectArea");

                position.width = (int)(RectWidth * (SystemParameters.PrimaryScreenWidth / test.ActualWidth));
                position.height = (int)(RectHeight * (SystemParameters.PrimaryScreenHeight / test.ActualHeight));
                position.top = (int)(Ypos * (SystemParameters.PrimaryScreenHeight / test.ActualHeight));
                position.left = (int)(Xpos * (SystemParameters.PrimaryScreenWidth / test.ActualWidth));

                //Console.WriteLine("Width:{0}, Height:{1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
                //Console.WriteLine("Width2:{0}, Height2:{1}", test.ActualWidth, test.ActualHeight);
                Console.WriteLine("RectangleString:{0}", RectangleMargin);
                Console.WriteLine("Xpos:{0}, Ypos:{1}", Xpos, Ypos);
            }
        }
    }
}
