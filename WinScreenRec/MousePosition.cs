using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinScreenRec
{
    class MousePosition
    {
        public struct Position
        {
            public int left;
            public int top;
            public int width;
            public int height;
        }

        private System.Windows.Point InitPos = new System.Windows.Point();


        public void SetInit(System.Windows.Point point)
        {
            InitPos = point;
        }

        public Position DrawRectangle(System.Windows.Point point, System.Windows.Controls.Canvas canvas)
        {
            System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
            Position position = new Position();

            rectangle.Stroke = new SolidColorBrush(Colors.Red);
            rectangle.StrokeThickness = 1;

            var width = Math.Abs(InitPos.X - point.X);
            var height = Math.Abs(InitPos.Y - point.Y);
            rectangle.Width = width;
            rectangle.Height = height;

            if (point.X > canvas.ActualWidth)
            {
                width = canvas.ActualWidth - InitPos.X;
                Canvas.SetLeft(rectangle, InitPos.X);
                rectangle.Width = width;
            }
            else if (point.X < 0)
            {
                width = InitPos.X;
                Canvas.SetLeft(rectangle, 0);
                rectangle.Width = width;
            }
            else if (InitPos.X < point.X)
            {
                Canvas.SetLeft(rectangle, InitPos.X);
                position.left = (int)(InitPos.X);
            }
            else
            {
                Canvas.SetLeft(rectangle, point.X);
                position.left = (int)(point.X);
            }



            return position;
        }
    }
}
