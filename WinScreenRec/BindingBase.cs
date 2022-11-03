using Reactive.Bindings;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;


namespace WinScreenRec
{
    class BindingBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// プロパティが変更されたことを通知する.
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private CompositeDisposable Disposable { get; }
        public ReactiveCommand<EventArgs> WindowClosedCommand { get; }

        public ReactiveProperty<int> MouseX { get; private set; }
        public ReactiveProperty<int> MouseY { get; private set; }

        public ReactiveCommand<Object> MouseMoveCommand { get; }

    }
}

