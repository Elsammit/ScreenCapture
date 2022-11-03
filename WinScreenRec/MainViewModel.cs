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
using System.Windows.Threading;
using Reactive.Bindings;
using System.Windows.Input;

namespace WinScreenRec
{
    class MainViewModel : BindingBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        bool isStartRec = false;
        bool isStartPrev = true;
        //bool isDrag = false;
        //bool isDragMoved = false;
        double RectTop = 0.0;
        double RectLeft = 0.0;
        bool IsMouseDown = false;
        Thread thread;

        ImgProcess m_ImgProcess = new ImgProcess();
        ImgProcess.RECT m_RECT = new ImgProcess.RECT();

        MousePosition m_MousePosition = new MousePosition();
        private UIElement canvasStock = new UIElement();
        WinScreenRec.MousePosition.Position position =
                    new WinScreenRec.MousePosition.Position();

        public MainViewModel()
        {
            thread = new Thread(new ThreadStart(() =>
            {
                CaptureMovieAsync();
            }));
            thread.Start();
            ButtonToRecStop();
        }

        private String _StartBtnContent = "";
        public String StartBtnContent {
            get => _StartBtnContent;
            set
            {
                _StartBtnContent = value;
                OnPropertyChanged(nameof(StartBtnContent));
            }
        }

        private DelegateCommand _StartBtnOnClick = null;
        public DelegateCommand StartBtnOnClick 
        {
            get
            {
                if(_StartBtnOnClick == null)
                {
                    _StartBtnOnClick = new DelegateCommand(StartRecordFunc, CanExecute);
                }
                return _StartBtnOnClick;
            }
        }

        private double _RectWidth = 0.0;
        public double RectWidth
        {
            get
            {
                return _RectWidth;
            }
            set
            {
                _RectWidth = value;
                OnPropertyChanged(nameof(RectWidth));
            }
        }

        private double _RectHeight = 0.0;
        public double RectHeight
        {
            get
            {
                return _RectHeight;
            }
            set
            {
                _RectHeight = value;
                OnPropertyChanged(nameof(RectHeight));
            }
        }

        private ReactiveCommand<Object> _MouseLeftBtnDwn = null;
        public ReactiveCommand<Object> MouseLeftBtnDwn 
        {
            get
            {
                if (_MouseLeftBtnDwn == null)
                {
                    _MouseLeftBtnDwn = new ReactiveCommand<Object>().WithSubscribe(obj => {
                        var ele = (IInputElement)obj;
                        var pos = Mouse.GetPosition(ele);
                        RectangleMargin = pos.X.ToString() + "," + pos.Y.ToString();
                        Console.WriteLine("Start MakeRectangle");
                        Console.WriteLine("X:{0}, Y:{1}", pos.X, pos.Y);

                        IsMouseDown = true;
                        RectTop = pos.Y;
                        RectLeft = pos.X;
                    });
                }
                return _MouseLeftBtnDwn;
            }
        }

        private DelegateCommand _MouseLeftBtnUp = null;
        public DelegateCommand MouseLeftBtnUp
        {
            get
            {
                if (_MouseLeftBtnUp == null)
                {
                    _MouseLeftBtnUp = new DelegateCommand(ButtonUpFunc, CanExecute);
                }
                return _MouseLeftBtnUp;
            }
        }

        private void ButtonUpFunc()
        {
            Console.WriteLine("Button Up");
            IsMouseDown = false;
        }

        private System.Windows.Media.ImageSource _ImageArea = null;
        public System.Windows.Media.ImageSource ImageArea {
            get => _ImageArea;
            set
            {
                _ImageArea = value;
                OnPropertyChanged(nameof(ImageArea));
            }
        }

        private String _RectangleMargin;
        public String RectangleMargin
        {
            get
            {
                return _RectangleMargin;
            }
            set
            {
                _RectangleMargin = value;
                OnPropertyChanged(nameof(RectangleMargin));
            }
        }

        private ReactiveCommand<Object> _MouseMoveCommand = null;
        public ReactiveCommand<Object> MouseMoveCommand
        {
            get
            {
                if (_MouseMoveCommand == null)
                {
                    _MouseMoveCommand = new ReactiveCommand<Object>().WithSubscribe(obj => {
                        var ele = (IInputElement)obj;
                        var pos = Mouse.GetPosition(ele);
                        double Xpos = RectLeft;
                        double Ypos = RectTop;
                        if (IsMouseDown)
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

                            Console.WriteLine("width:{0}, height:{1}", RectWidth, RectHeight);
                            Console.WriteLine("Xpos:{0}, Ypos:{1}", Xpos, Ypos);
                            //MainViewModel.m_controlModel.SetRect((int)Ypos, (int)Xpos, (int)RectHeight, (int)RectWidth);
                        }
                    });
                }


                return _MouseMoveCommand;
            }
        }

        private double _RecBorderOpacity = 0;
        public double RecBorderOpacity {
            get => _RecBorderOpacity;
            set
            {
                _RecBorderOpacity = value;
                OnPropertyChanged(nameof(RecBorderOpacity));
            }
        }

        private String _RecTimerContent = "";
        public String RecTimerContent {
            get => _RecTimerContent;
            set
            {
                _RecTimerContent = value;
                OnPropertyChanged(nameof(RecTimerContent));
            }
        }

        private double _RecTimerOpacity = 0;
        public double RecTimerOpacity {
            get => _RecTimerOpacity;
            set
            {
                _RecTimerOpacity = value;
                OnPropertyChanged(nameof(RecTimerOpacity));
            }
        }


        private void StartRecordFunc()
        {
            Console.WriteLine("Start Record Func");
            if (isStartRec)
            {
                ButtonToRecStop();
            }
            else
            {
                var dialog = new SaveFileDialog();
                dialog.Title = "ファイルを保存";
                dialog.Filter = "動画ファイル|*.wmv";
                // ダイアログを表示
                if (dialog.ShowDialog() == true)
                {
                    m_ImgProcess.SetFilePath(dialog.FileName, m_RECT);
                    ButtonToRecStart();
                }
            }
        }

        private void ButtonToRecStop()
        {
            isStartRec = false;
            StartBtnContent = "録画開始";
            RecBorderOpacity = 0;
            RecTimerOpacity = 0;
        }

        private void ButtonToRecStart()
        {
            isStartRec = true;
            StartBtnContent = "録画停止";
            RecBorderOpacity = 100;
            RecTimerOpacity = 100;
        }

        private bool CanExecute()
        {
            return true;
        }

        private void CaptureMovieAsync()
        {
            int timerCnt = 0;
            while (isStartPrev)
            {
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
                var bitmap = m_ImgProcess.GetCaptureImage(isStartRec, m_RECT);
                var hBitmap = bitmap.GetHbitmap();


                if (isStartRec)
                {
                    timerCnt++;
                }
                else
                {
                    timerCnt = 0;
                }

                int sec = (timerCnt / 10) % 60;
                int minute = (timerCnt / 10) / 60;

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    ImageArea = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                    RecTimerContent = minute.ToString("00") + ":" + sec.ToString("00");

                    if (timerCnt >= 18000)
                    {
                        isStartRec = false;
                        timerCnt = 0;

                        //var win = new CustomMsgBox();
                        //win.Owner = this;
                        //win.ShowDialog();

                        //ButtonToRecStop();

                    }
                }));
                DeleteObject(hBitmap);
                Cv2.WaitKey(80);
            }
        }

    }
}
