using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using static OpenCvSharp.Stitcher;

namespace OCTGui.ViewModels
{
    public class vmOctControl : ObservableObject
    {
        public event EventHandler RequestClose;

        public vmOctControl(bool status)
        {
            Status = status;
            setUpControl();
        }

        private bool _status;
        public bool Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        private void setUpControl()
        {
            Debug.WriteLine(Status);
            if (Status == true)
            {
                OCTIsActive = true;
            }
            if (Status == false) 
            {
                OCTIsActive = false;
            }
        }


        private RelayCommand _okCommand = null!;
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new RelayCommand(() => executeOkCommand(), () => canExecuteOkCommand());
                return _okCommand;
            }
        }
        private bool canExecuteOkCommand()
        {
            return true;
        }

        private bool checkPassword()
        {
            if (Password == "123") 
                return true;
            else return false;
        }

        public void executeOkCommand()
        {
            Console.Beep();
            if (Status)
            {
                if (checkPassword() == true)
                {
                    OCTIsActive = false;
                    RequestClose?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Password is incorrect","Password Error",MessageBoxButton.OK);
                    return;
                }
            }
            if (!Status)
            {
                OCTIsActive = true;
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
        }



        private bool _octIsActive;
        public bool OCTIsActive
        {
            get => _octIsActive;
            set
            {
                if (_octIsActive != value)
                {
                    _octIsActive = value;

                }
            }
        }


        public string Password { private get; set; }

    }
}
