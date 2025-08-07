using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp.WpfExtensions;
using System.Runtime.InteropServices;
using OCTGui.ViewModels;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Text.Json;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml;
using System.Text.Json.Serialization;
using System.Net.Sockets;
using static OCTGui.ViewModels.vmOctControl;
using CommunityToolkit.Mvvm.Messaging.Messages;
using static OpenCvSharp.Stitcher;
using static OCTGui.ViewModels.vmWorkspace;
using System.Windows.Automation;
using System.Net.NetworkInformation;
using OCTGui.Devices;
using OCTGui.Transport;
using System.IO.Enumeration;








namespace OCTGui.ViewModels
{
    public class vmWorkspace : ObservableObject
    {
        public vmWorkspace()
        {
            LoadProfiles();
            LoadConnection();
            startBackgroundWorker();
            var window = System.Windows.Window.GetWindow(Application.Current.MainWindow);
            window.KeyDown += KeyDown;
            window.KeyUp += KeyUp;
        }



        public event EventHandler OnMainWindowClosed;
        public void Close()
        {
            Debug.WriteLine("Close() called");
            GPIODemo.stop();
            Laserdiode.Stop();
            OctProZ.Stop();
            _workerRuns.Cancel();
            workerThread.Join();
            OnMainWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private RelayCommand _updateCommand = null!;
        public ICommand UpdateCommand
        {
            get
            {
                if (_updateCommand == null)
                    _updateCommand = new RelayCommand(() => executeUpdateCommand(), () => canExecuteUpdateCommand());
                return _updateCommand;
            }
        }
        private bool canExecuteUpdateCommand()
        {
            return true;
        }
        private void executeUpdateCommand()
        {
            Console.Beep();
            SaveProfiles();
        }

        public class ProfileModel()
        {
            private string _profile;
            private double _pulselength;
            private double _pulseRate;
            private double _laserCurrent;
            private string _notes;
            public string Profile
            {
                get => _profile;
                set { _profile = value; OnPropertyChanged(nameof(Profile)); }
            }
            public double Pulselength
            {
                get => _pulselength;
                set
                {
                    _pulselength = value;
                    OnPropertyChanged(nameof(Pulselength));
                    if (_pulselength < 40) Pulselength = 40;
                    if (_pulselength > 1000) Pulselength = 1000;
                }
            }
            public double PulseRate
            {
                get => _pulseRate;
                set
                {
                    _pulseRate = value;
                    OnPropertyChanged(nameof(PulseRate));
                    if (_pulseRate < 5) PulseRate = 5;
                    if (_pulseRate > 1000) PulseRate = 1000;
                }
            }
            public double LaserCurrent
            {
                get => _laserCurrent;
                set { _laserCurrent = value; OnPropertyChanged(nameof(LaserCurrent)); }
            }
            public string Notes
            {
                get => _notes;
                set { _notes = value; OnPropertyChanged(nameof(Notes)); }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        private const string FilePath = "Laserparameter.json";
        private ProfileModel _selectedProfile;
        public ObservableCollection<ProfileModel> Profiles { get; set; } = new ObservableCollection<ProfileModel>();

        public ProfileModel SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                if (_selectedProfile != value)
                {
                    _selectedProfile = value;
                    OnPropertyChanged(nameof(SelectedProfile));
                }
            }
        }
        private void LoadProfiles()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var profileList = JsonSerializer.Deserialize<List<ProfileModel>>(json);
                if (profileList != null)
                {
                    foreach (var profile in profileList)
                        Profiles.Add(profile);
                    SelectedProfile = Profiles.FirstOrDefault();
                }
            }
        }

        private void SaveProfiles()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Profiles.ToList(), options);
            File.WriteAllText(FilePath, json);
        }

        private RelayCommand _toggleFullscreen = null!;
        public ICommand ToggleFullscreen
        {
            get
            {
                if (_toggleFullscreen == null)
                    _toggleFullscreen = new RelayCommand(() => toggleFullScreen(), () => canToggleFullScreen());
                return _toggleFullscreen;
            }
        }
        public void toggleFullScreen()
        {
            Console.Beep();
            Fullscreen = !Fullscreen;
        }
        private bool canToggleFullScreen()
        {
            return true;
        }


        private bool _Fullscreen;
        public bool Fullscreen
        {
            get => _Fullscreen;
            set { _Fullscreen = value; OnPropertyChanged(nameof(Fullscreen)); }
        }


        private bool _notReady = true;

        public bool NotReady
        {
            get => _notReady;
            set
            {
                if (_notReady != value)
                {
                    _notReady = value;
                    OnPropertyChanged();
                    canStartSavingVideo();
                }
            }
        }

        

        private bool _wantToSaveVideo;
        public bool WantToSaveVideo
        {
            get => _wantToSaveVideo;
            set
            {
                if (_wantToSaveVideo != value)
                {
                    _wantToSaveVideo = value;
                    OnPropertyChanged();
                    canStartSavingVideo();
                }
            }
        }
        private bool _videoIsRecording;
        public bool VideoIsRecording
        {
            get => _videoIsRecording; set => SetProperty(ref _videoIsRecording, value);
        }

        public event EventHandler ToggleTheRecording;
        private void canStartSavingVideo()
        {
            if (VideoIsRecording)
            {
                ToggleTheRecording?.Invoke(this, EventArgs.Empty);
                VideoIsRecording = false;
                return;
            }
            if (WantToSaveVideo && !NotReady)
            {
                ToggleTheRecording?.Invoke(this, EventArgs.Empty);
                VideoIsRecording = true;
                return;
            }
            return;
        }
        private RelayCommand _readyCommand = null!;
        public ICommand ReadyCommand
        {
            get
            {
                if (_readyCommand == null)
                    _readyCommand = new RelayCommand(() => executeReadyCommand(), () => canExecuteReadyCommand());
                return _readyCommand;
            }
        }

        private bool canExecuteReadyCommand()
        {
            return true;
        }


        private RelayCommand _octControlCommand = null!;
        public RelayCommand OctControlCommand
        {
            get
            {
                if (_octControlCommand == null)
                    _octControlCommand = new RelayCommand(() => executeOCTControlCommand(), () => canExecuteOCTControlCommand());
                return _octControlCommand;
            }
        }
        private bool canExecuteOCTControlCommand()
        {
            return true;
        }
        private bool _showOctControl;
        public bool ShowOctControl
        {
            get => _showOctControl; set => SetProperty(ref _showOctControl, value);
        }

        vmOctControl vmControl;
        private void executeOCTControlCommand()
        {
            OctControl controlBox = new OctControl();
            controlBox.Owner = Application.Current.MainWindow;
            vmControl = new vmOctControl(StatusOCT);
            controlBox.DataContext = vmControl;
            controlBox.ShowDialog();
            StatusOCT = vmControl.OCTIsActive;
            vmControl = null!;
        }
        private bool _statusOCT = Properties.Settings.Default.OctActivationStatus;
        public bool StatusOCT
        {
            get => _statusOCT;
            set => SetProperty(ref _statusOCT, value);
        }
        private void executeReadyCommand()
        {
            Thread _LaserDiodeThread = null!;
            Thread _OCTThread = null!;
            if (NotReady)
            {
                wsStatusUpdate = 2;
                Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                GPIODemo.start();
                if (StatusOCT)
                {
                    _OCTThread = new Thread(OctProZ.Start);
                    _OCTThread.IsBackground = true;
                    _OCTThread.Start();
                    OctProZ._OCTThread = _OCTThread;
                }
                //_LaserDiodeThread = new Thread(Laserdiode.Start);
                _LaserDiodeThread = new Thread(Laserdiode.empty);
                _LaserDiodeThread.IsBackground = true;
                _LaserDiodeThread.Start();
                Laserdiode._LaserDiodeThread = _LaserDiodeThread;
                NotReady = false;
            }
            else
            {
                wsStatusUpdate = 0;
                NotReady = true;
                Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                GPIODemo.stop();
                Laserdiode.Stop();
                OctProZ.Stop();
            }
        }

        private int _wsStatusUpdate;
        public int wsStatusUpdate
        {
            get => _wsStatusUpdate;
            set
            {
                if (_wsStatusUpdate != value)
                {
                    _wsStatusUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        CancellationTokenSource _workerRuns = new CancellationTokenSource();
        Thread workerThread;

        private void startBackgroundWorker()
        {
            workerThread = new Thread(worker);
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        private int _octSignalLamp;
        public int OctSignalLamp
        {
            get => _octSignalLamp;set => SetProperty(ref _octSignalLamp, value);
        }

        private bool _canRunLaser = false;
        public bool CanRunLaser
        {
            get => _canRunLaser;set => SetProperty(ref _canRunLaser, value);
        }


        private void StopFunction()
        {
            NotReady = true;
            wsStatusUpdate = 3;
            GPIODemo.stop();
            Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
            Laserdiode.Stop();
            OctProZ.Stop();
        }
        vmErrorBox vmerrorBox;
        private void showErrorMessage(bool StatusOfOct, string message, bool ErrorIsOct)
        {
            ErrorBox errorBox = new ErrorBox();
            errorBox.Owner = Application.Current.MainWindow;
            vmerrorBox = new vmErrorBox(StatusOfOct,message, ErrorIsOct);
            errorBox.DataContext = vmerrorBox;
            errorBox.ShowDialog();
            StatusOCT = vmerrorBox.StatusOct;
            bool continueProcess = vmerrorBox.ContWithProcess;
            vmerrorBox = null!;
            wsStatusUpdate = 0;
            if (ErrorIsOct && !StatusOCT && continueProcess)
            {
                executeReadyCommand();
            }
        }

        private bool _logging = false;
        public bool Logging
        {
            get => _logging; set => SetProperty(ref _logging, value);
        }
        private DateTime loggingStartTime = DateTime.MinValue;
        private void worker()
        {
            while (!_workerRuns.IsCancellationRequested)
            {

                var _WSfootswitch = FootSwitch;
                var _WSoctstatus = StatusOCT;
                var _octtimeofconnection = OctProZ.TimeOfConnection;
                var _octtime = OctProZ.LastReceivedMessageTime;
                var _octLastReceivedMessage = OctProZ.Data;
                var _octresult = OctProZ.OctSignal;
                var _octconnection = OctProZ.ConnectionStatus;
                var _laserdiodeconnection  = Laserdiode.ConnectionStatus;
                var _laserdiodeupdatetime = Laserdiode.LastUpdateTime;
                var _laserdiodeLastReceivedMessage = Laserdiode.Data;
                var _WSisReady = !NotReady;
                bool _octLaserCanRun = false;
                DateTime timeNow = DateTime.Now;
                var octtimeDif = timeNow - _octtime;
                var octcontimeDif = timeNow - _octtimeofconnection;
                var laserdiodetimeDif = timeNow - _laserdiodeupdatetime;
                string loggingFileName;


                /// Oct Signal Lampe
                if (_WSoctstatus)
                {
                    if(_octresult == "Ready")
                    {
                        _octLaserCanRun = true;
                        OctSignalLamp = 1;
                    }
                    else if (_octresult == "notReady")
                    {
                        OctSignalLamp = 3;
                    }
                    else
                    {
                        OctSignalLamp = 2;
                    }
                }
                if (!_WSoctstatus)
                {
                    OctSignalLamp = 0;
                }
                

                /// Verbindungsstatus
                if (!_octconnection)
                {
                    /// OCT Fehler
                    string errorMessage = "Could not start the connection to OctProZ";
                    StopFunction();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        showErrorMessage(_WSoctstatus, errorMessage, true);
                    });
                }
                if (!_laserdiodeconnection)
                {
                    /// Laserdioden Fehler
                    string errorMessage = "Could not start the connection to the Laserdiode";
                    StopFunction();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        showErrorMessage(_WSoctstatus, errorMessage, false);
                    });
                }
                if (_WSisReady)
                {
                    if (_WSoctstatus)
                    {
                        if(_WSfootswitch && _octLaserCanRun)
                        {
                            CanRunLaser = true;
                            GPIODemo.send(CanRunLaser);

                        }
                        if(!_WSfootswitch || !_octLaserCanRun)
                        {
                            CanRunLaser = false;
                            GPIODemo.send(CanRunLaser);

                        }
                    }
                    if (!_WSoctstatus)
                    {
                        if (_WSfootswitch)
                        {
                            CanRunLaser = true;
                            GPIODemo.send(CanRunLaser);
                        }
                        if (!_WSfootswitch)
                        {
                            CanRunLaser = false;
                            GPIODemo.send(CanRunLaser);
                        }
                    }
                    if (octtimeDif.TotalMilliseconds > Properties.Settings.Default.OCTProZTimeOut && _octtime != DateTime.MinValue)
                    {
                        /// OCT Fehler
                        StopFunction();
                        string errorMessage = "Since the last received message from OCTProZ 10 seconds have passed, the connection timed out."; // muss ich noch besser schreiben
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            showErrorMessage(_WSoctstatus, errorMessage, true);
                        });
                    }
                    if(octcontimeDif.TotalMilliseconds > Properties.Settings.Default.OCTProZTimeOut && _octtimeofconnection != DateTime.MinValue && _octresult == null)
                    {
                        /// OCT Fehler
                        StopFunction();
                        string errorMessage = "We haven’t received any messages from OCTProZ since the connection was established."; // muss ich noch besser schreiben
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            showErrorMessage(_WSoctstatus, errorMessage, true);
                        });

                    }
                    if(laserdiodetimeDif.TotalMilliseconds > Properties.Settings.Default.LaserDiodeTimeOut && _laserdiodeupdatetime != DateTime.MinValue)
                    {
                        /// Laserdioden Fehler
                        string errorMessage = "We haven’t received any messages from Laserdiode for an extended period of time."; 
                        StopFunction();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            showErrorMessage(_WSoctstatus, errorMessage, false);
                        });
                    }
                }

                /// Logging
                if (Logging)
                {
                    if (loggingStartTime == DateTime.MinValue)
                    {
                        loggingStartTime = DateTime.Now;
                        string timestamp = loggingStartTime.ToString("yyyyMMdd_HHmmss");
                        loggingFileName = $"logging_{timestamp}.txt";
                        Debug.WriteLine(loggingFileName);
                        string loggingHeader = "Time|\tOct status|\tOct connection time|\tOct last message time|\tOct last message|\tOct laser can run|\tLaserdiode update time|\tLaserdiode last message|\tLaser can run|\tFootswitch\n";
                        //Debug.WriteLine(loggingHeader);
                        //string filepath = Properties.Settings.Default.LogSaveFilePath;
                        //string fullfilepath = System.IO.Path.Combine(filepath, "logging");
                        //System.IO.Directory.CreateDirectory(fullfilepath);
                        //string outputFile = System.IO.Path.Combine(fullfilepath, loggingFileName);
                        //string loggingContent = $"{timeNow}|\t{_WSoctstatus}|\t{_octtimeofconnection}|\t{_octtime}|\t{_octLastReceivedMessage}|\t{_octLaserCanRun}|\t{_laserdiodeupdatetime}|\t{_laserdiodeLastReceivedMessage}|\t{CanRunLaser}|\t{_WSfootswitch}\n";
                        //Debug.WriteLine(loggingContent);
                    }
                    //string loggingContent = $"{timeNow}|\t{_WSoctstatus}\t{_octtimeofconnection}|\t{_octtime}|\t{_octLastReceivedMessage}|\t{_octLaserCanRun}\t{_laserdiodeupdatetime}|\t{_laserdiodeLastReceivedMessage}|\t{CanRunLaser}|\t{_WSfootswitch}\n";
                    //Debug.WriteLine(loggingContent);
                    //using (StreamWriter writer = new StreamWriter("outputFile", append: true))
                    //{
                    //    writer.WriteLine(loggingContent);
                    //}
                }
                if (!Logging) 
                { 
                    if(loggingStartTime != DateTime.MinValue)
                    {
                        loggingStartTime = DateTime.MinValue;
                    }
                }
                if (!_WSisReady)
                {
                    if (_WSoctstatus)
                    {
                        OctSignalLamp = 2; // back to idle
                    }
                    CanRunLaser = false; 
                }
            }
            Debug.WriteLine("Worker Closed");
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.D1 || e.Key == Key.NumPad1)&& !NotReady)
            {
                FootSwitch = true;
            }
        }
        private void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            {
                FootSwitch = false;
            }
        }
        public bool _footSwitch;
        public bool FootSwitch
        {
            get => _footSwitch;
            set
            {
                if (_footSwitch != value)
                {
                    _footSwitch = value;
                    OnPropertyChanged();
                }
            }
        }


        private OCTProZ OctProZ;
        private LaserDiode Laserdiode;
        private GPIO_Demo GPIODemo;

        private void LoadConnection()
        {
            if (Properties.Settings.Default.OCTProZ.Split(':')[0] == "TCP")
            {
                var parts = Properties.Settings.Default.OCTProZ.Split(':');
                OctProZ = new OCTProZ(new TcpTransportDev(parts[1], int.Parse(parts[2])));
            }
            if (Properties.Settings.Default.OCTProZ.Split(':')[0] == "COM")
            {
                var parts = Properties.Settings.Default.OCTProZ.Split(':');
                OctProZ = new OCTProZ(new COMTransportDev(
                    parts[1],
                    int.Parse(parts[2]),
                    Enum.Parse<Parity>(parts[3]),
                    int.Parse(parts[4]),
                    Enum.Parse<StopBits>(parts[5])
                    ));
            }
            if (Properties.Settings.Default.LaserDiode.Split(":")[0] == "TCP")
            {
                var parts = Properties.Settings.Default.LaserDiode.Split(':');
                Laserdiode = new LaserDiode(new TcpTransportDev(parts[1], int.Parse(parts[2])));
            }
            if (Properties.Settings.Default.LaserDiode.Split(':')[0] == "COM")
            {
                var parts = Properties.Settings.Default.LaserDiode.Split(':');
                Laserdiode = new LaserDiode(new COMTransportDev(
                    parts[1],
                    int.Parse(parts[2]),
                    Enum.Parse<Parity>(parts[3]),
                    int.Parse(parts[4]),
                    Enum.Parse<StopBits>(parts[5])
                    ));
            }

            /// GPIO Demo device
            /// 
            if (Properties.Settings.Default.GPIO_Demo.Split(":")[0] == "TCP")
            {
                var parts = Properties.Settings.Default.GPIO_Demo.Split(':');
                GPIODemo = new GPIO_Demo(new TcpTransportDev(parts[1], int.Parse(parts[2])));
            }
            if (Properties.Settings.Default.GPIO_Demo.Split(':')[0] == "COM")
            {
                var parts = Properties.Settings.Default.GPIO_Demo.Split(':');
                GPIODemo = new GPIO_Demo(new COMTransportDev(
                    parts[1],
                    int.Parse(parts[2]),
                    Enum.Parse<Parity>(parts[3]),
                    int.Parse(parts[4]),
                    Enum.Parse<StopBits>(parts[5])
                    ));
            }
        }




    }
}
