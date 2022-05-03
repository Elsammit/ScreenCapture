﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private UIElement canvasStock = new UIElement();

        public bool SetInit(System.Windows.Point point, Canvas canvas)
        {
            bool ret = false;

            Console.WriteLine("init Left:{0}, top:{1}", point.X, point.Y);
            if (point.Y > canvas.Margin.Top && point.Y < (canvas.Margin.Top + canvas.Height) &&
                point.X > canvas.Margin.Left && point.X < canvas.Margin.Left + canvas.Width)
            {
                InitPos = point;
                ret = true;
            }
            return ret;
        }

        public bool DrawRectangle(System.Windows.Point point, double canvasWidth, double canvasHeight,
            ref Position position, ref System.Windows.Shapes.Rectangle rectangle)
        {

            bool ret = false;

            rectangle.Stroke = new SolidColorBrush(Colors.Red);
            rectangle.StrokeThickness = 1;

            var width = Math.Abs(InitPos.X - point.X);
            var height = Math.Abs(InitPos.Y - point.Y);
            rectangle.Width = width;
            rectangle.Height = height;

            if (point.X > canvasWidth)
            {
                width = canvasWidth - InitPos.X;
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

            if (point.Y > canvasHeight)
            {
                Console.WriteLine("UpUp");
                height = canvasHeight - InitPos.Y;
                Canvas.SetTop(rectangle, InitPos.Y);
                rectangle.Height = height;
            }
            else if (point.Y < 0)
            {
                Console.WriteLine("DownDown");
                height = InitPos.Y;
                Canvas.SetTop(rectangle, 0);
                rectangle.Height = height;
            }
            else if (InitPos.Y < point.Y)
            {
                Canvas.SetTop(rectangle, InitPos.Y);
                position.top = (int)(InitPos.Y);
            }
            else
            {
                Canvas.SetTop(rectangle, point.Y);
                position.top = (int)(point.Y);
            }

            position.width = (int)(width * SystemParameters.PrimaryScreenWidth / canvasWidth);
            position.height = (int)(height / (int)canvasHeight * (int)SystemParameters.PrimaryScreenHeight);
            position.top = position.top * (int)(SystemParameters.PrimaryScreenHeight / canvasHeight);
            position.left = position.left * (int)(SystemParameters.PrimaryScreenWidth / canvasWidth);

            return ret;
        }
    }
}
