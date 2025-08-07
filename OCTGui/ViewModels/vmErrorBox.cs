using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Channels;
using System.Windows;
using OpenCvSharp;
using System.Windows.Controls;

namespace OCTGui.ViewModels
{
    internal class vmErrorBox : ObservableObject
    {

        public event EventHandler RequestClose;
        public vmErrorBox(bool StatusOfOct, string message, bool ErrorIsOct)
        {
            StatusOct = StatusOfOct;
            ItIsOct = ErrorIsOct;
            ErrorMessage = message;
            IsNotOct = !ErrorIsOct;
        }
        private bool _isNotOct;
        public bool IsNotOct
        {
            get => _isNotOct; set => SetProperty(ref _isNotOct, value);
        }


        public ImageSource ErrorIcon
        {
            get
            {
                using var icon = SystemIcons.Error;
                using var ms = new MemoryStream();
                icon.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                return image;
            }
        }

        private bool _ContWithoutOCTBool = false;
        public bool ContWithoutOCTBool
        {
            get => _ContWithoutOCTBool; set => SetProperty(ref _ContWithoutOCTBool, value);
        }


        private RelayCommand _contWithoutOCT = null!;
        public ICommand ContWithoutOCT
        {
            get
            {
                if (_contWithoutOCT == null)
                    _contWithoutOCT = new RelayCommand(() => executeContWithoutOCT(), () => canExecuteContWithoutOCT());
                return _contWithoutOCT;
            }
        }
        private bool canExecuteContWithoutOCT()
        {
            return true;
        }
        public void executeContWithoutOCT()
        {
            ItIsOct = false;
            ContWithoutOCTBool = true;
        }

        private bool checkPassword()
        {
            if (Password == "123")
                return true;
            else return false;
        }

        private RelayCommand _deactivateOct = null!;
        public ICommand DeactivateOct
        {
            get
            {
                if (_deactivateOct == null)
                    _deactivateOct = new RelayCommand(() => executeDeactivationOct(), () => canExecuteDeactivationOct());
                return _deactivateOct;
            }
        }
        private bool canExecuteDeactivationOct()
        {
            return true;
        }
        public void executeDeactivationOct()
        {
            if (checkPassword() == true)
            {
                StatusOct = false;
                RequestClose?.Invoke(this, EventArgs.Empty);
                
            }
            else
            {
                MessageBox.Show("Password is incorrect", "Password Error", MessageBoxButton.OK);
                return;
            }
        }

        private bool _contWithProcess = true;
        public bool ContWithProcess
        {
            get => _contWithProcess; set => SetProperty(ref _contWithProcess, value);
        }



        public string Password { private get; set; }

        private bool _statusOct;
        public bool StatusOct
        {
            get => _statusOct; set => SetProperty(ref _statusOct, value);
        }

        private bool _itIsOct;
        public bool ItIsOct
        {
            get => _itIsOct; set => SetProperty(ref _itIsOct, value);
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage; set => SetProperty(ref _errorMessage, value);
        }







    }
}
