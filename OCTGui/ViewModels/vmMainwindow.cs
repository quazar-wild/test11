using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCTGui.ViewModels;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace OCTGui
{
   
    class vmMainwindow : ObservableObject
    {
        public vmMainwindow()
        {
            CurrentContent = new vmLogin();
        }


        private RelayCommand _AboutBoxCommand = null!;
        public ICommand AboutBoxCommand
        {
            get
            {
                if (_AboutBoxCommand == null)
                    _AboutBoxCommand = new RelayCommand(() => executeAboutBoxCommand(), () => canExecuteAboutBoxCommand());
                return _AboutBoxCommand;
            }
        }
        private void executeAboutBoxCommand()
        {
            AboutBox about = new AboutBox();
            about.Owner = Application.Current.MainWindow;
            about.ShowDialog();
        }
        private bool canExecuteAboutBoxCommand()
        {
            return true;
        }



        private int _statusUpdate = 0;
        public int StatusUpdate
        {
            get => _statusUpdate; 
            set { _statusUpdate = value; OnPropertyChanged(); }
        }

        private bool _Fullscreen = false;
        public bool Fullscreen
        {
            get => _Fullscreen;
            set { _Fullscreen = value; OnPropertyChanged(); }
        }
        private WindowState _previousWindowState;
        private WindowState _CurrentWindowState = Enum.Parse<WindowState>(Properties.Settings.Default.WindowState); 
        public WindowState CurrentWindowState
        {
            get => _CurrentWindowState;
            set { _CurrentWindowState = value; OnPropertyChanged(); }
        }
        private WindowStyle _previousWindowStyle;
        private WindowStyle _CurrentWindowStyle = WindowStyle.SingleBorderWindow;
        public WindowStyle CurrentWindowStyle
        {
            get => _CurrentWindowStyle;
            set { _CurrentWindowStyle = value; OnPropertyChanged(); }
        }
        public void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("Wollen Sie die Anwendung wirklich schließen? \n Alle Prozesse werden beendet.", "Anwendung schließen", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    vmWorkspace doc = CurrentContent as vmWorkspace;
                    if (doc != null) doc.Close();
                    e.Cancel = false;
                }
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
        }
        private bool _loggedIn = false;
        public bool loggedIn
        {
            get => _loggedIn;
            set
            {
                if (_loggedIn != value)
                {
                    _loggedIn = value;
                    OnPropertyChanged();
                }
            }
        }
        private RelayCommand _LogInCommand = null!;
        public ICommand LogInCommand
        {
            get
            {
                if (_LogInCommand == null)
                    _LogInCommand = new RelayCommand(() => executeLogInCommand(), () => canExecuteLogInCommand());
                return _LogInCommand;
            }
        }
        private bool canExecuteLogInCommand()
        {
            return true;
        }
        private vmWorkspace _workspace;
        public void SwitchToWorkspace()
        {
            _workspace = new vmWorkspace();
            CurrentContent = _workspace;
            _workspace.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vmWorkspace.Fullscreen))
                {
                    Fullscreen = _workspace.Fullscreen;
                    if (Fullscreen)
                    {
                        _previousWindowStyle = Application.Current.MainWindow.WindowStyle;
                        _previousWindowState = Application.Current.MainWindow.WindowState;
                        CurrentWindowState = WindowState.Maximized;
                        CurrentWindowStyle = WindowStyle.None;
                    }
                    else
                    {
                        CurrentWindowState = _previousWindowState;
                        CurrentWindowStyle = _previousWindowStyle;
                    }
                }
                if (e.PropertyName == nameof(vmWorkspace.wsStatusUpdate))
                {
                    StatusUpdate = _workspace.wsStatusUpdate;
                }
            };
        }

        private string _logInButtonText = "Log In";
        public string LogInButtonText
        {
            get => _logInButtonText; set => SetProperty(ref _logInButtonText, value);
        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(obj =>
                {
                    ((DispatcherFrame)obj).Continue = false;
                    return null;
                }),
                frame);
            Dispatcher.PushFrame(frame);
        }

        private void executeLogInCommand()
        {
            //Console.Beep();
            if (loggedIn) 
            {
                var result = MessageBox.Show("Wollen Sie sich wirklich abmelden?", "Abmeldung", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                StatusUpdate = 3;
                OnPropertyChanged(nameof(StatusUpdate));
                DoEvents();
                _workspace.Close(); 
                loggedIn = false;
                LogInButtonText = "Log In";
                CurrentContent = new vmLogin();
                StatusUpdate = 0;
            }
            else
            {
                vmLogin _login = CurrentContent as vmLogin;
                if (_login.CanLogIn()) // prüfen des Passwords 
                {
                    StatusUpdate = 1;
                    OnPropertyChanged(nameof(StatusUpdate));
                    DoEvents();
                    loggedIn = true;
                    LogInButtonText = $"Log out: {_login.UserName}";
                    SwitchToWorkspace();
                    StatusUpdate = 0;
                }
                else return;
            }
        }

        private ObservableObject _CurrentContent; 
        public ObservableObject CurrentContent
        {
            get => _CurrentContent;
            set
            {
                if (_CurrentContent != value)
                {
                    _CurrentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            } 
        }


    }
}
