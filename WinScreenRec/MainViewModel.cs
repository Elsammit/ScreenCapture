using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Threading;
using Microsoft.Win32;
using Reactive.Bindings;
using System.Windows.Input;

namespace WinScreenRec
{
    class MainViewModel : BindingBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        Thread thread;

        MainModel m_MainModel = new MainModel();

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

        private DelegateCommand _ClickCloseWindow = null;
        public DelegateCommand ClickCloseWindow
        {
            get
            {
                if (_ClickCloseWindow == null)
                {
                    _ClickCloseWindow = new DelegateCommand(CloseWindowFunc, CanExecute);
                }
                return _ClickCloseWindow;
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
                        if (!m_MainModel.isStartRec)
                        {
                            var ele = (IInputElement)obj;
                            var pos = Mouse.GetPosition(ele);
                            RectangleMargin = pos.X.ToString() + "," + pos.Y.ToString();
                            m_MainModel.RectangleMargin = RectangleMargin;

                            m_MainModel.IsMouseDown = true;
                            m_MainModel.RectTop = pos.Y;
                            m_MainModel.RectLeft = pos.X;
                        }
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
            m_MainModel.IsMouseDown = false;
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

        public void SetRectInformation(double rectHeight, double rectWidth, string rectMargin)
        {
            RectHeight = rectHeight;
            RectWidth = rectWidth;
            RectangleMargin = rectMargin;
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
                        m_MainModel.MakePosition(pos, SetRectInformation);
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
        public double RecTimerOpacity
        {
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
            if (m_MainModel.isStartRec)
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
                    m_MainModel.SetFilePath(dialog.FileName);
                    ButtonToRecStart();
                }
            }
        }

        private void ButtonToRecStop()
        {
            m_MainModel.isStartRec = false;
            StartBtnContent = "録画開始";
            RecBorderOpacity = 0;
            RecTimerOpacity = 0;
        }

        private void ButtonToRecStart()
        {
            m_MainModel.isStartRec = true;
            StartBtnContent = "録画停止";
            RecBorderOpacity = 100;
            RecTimerOpacity = 100;
        }

        private void CloseWindowFunc()
        {
            m_MainModel.isStartPrev = false;
        }

        private bool CanExecute()
        {
            return true;
        }

        public void DispCapture(ref System.Drawing.Bitmap bitmap, int minute, int sec)
        {
            var hBitmap = bitmap.GetHbitmap();
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                ImageArea = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

                RecTimerContent = minute.ToString("00") + ":" + sec.ToString("00");
            }));
            DeleteObject(hBitmap);
        }

        private void CaptureMovieAsync()
        {
            m_MainModel.CaptureMovieAsync(DispCapture);
        }

    }
}
