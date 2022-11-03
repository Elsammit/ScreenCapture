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

namespace WinScreenRec
{
    class MainViewModel : BindingBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        bool isStartRec = false;
        bool isStartPrev = true;
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

        private System.Windows.Media.ImageSource _ImageArea = null;
        public System.Windows.Media.ImageSource ImageArea {
            get => _ImageArea;
            set
            {
                _ImageArea = value;
                OnPropertyChanged(nameof(ImageArea));
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
