using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using OCTGui.ViewModels;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;


namespace OCTGui;

    /// <summary>
    /// Interaction logic for WorkSpace.xaml
    /// </summary>
public partial class WorkSpace : UserControl
{
    private string videoRecordingFilePath = Properties.Settings.Default.LogSaveFilePath;
    private double camarafps = Properties.Settings.Default.CamaraFPS;
    private VideoCapture _capture;
    public WorkSpace()
    {
        InitializeComponent();
        StartCamera();
    }


    Thread _thread = null;
    CancellationTokenSource _cancellationTokenSource;

    
    private void StartCamera()
    {
        _capture = new VideoCapture(); // 0 = erste Kamera
        _capture.Open(0, VideoCaptureAPIs.DSHOW);
        _capture.Set(VideoCaptureProperties.Fps, camarafps); // das Video hat momentan maximale FPS bei ca. 30, kann an der Kamera liegen
        var counter = 0;
        var stopWatch = Stopwatch.StartNew();
        _cancellationTokenSource = new CancellationTokenSource();
        _thread = new Thread(() =>
        {
            Debug.WriteLine("start {0}",System.Environment.CurrentManagedThreadId);
            using (var mat = new Mat())
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {

                    if (!_capture.Read(mat))
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    counter++;
                    if (stopWatch.ElapsedMilliseconds > 0)
                    {
                        double frameRate = (double)counter / stopWatch.ElapsedMilliseconds * 1000;
                        Cv2.PutText(mat, $"FPS={frameRate:F}", new OpenCvSharp.Point(10, 30), HersheyFonts.HersheyPlain, 1, Scalar.Red);
                    }
                    if (!mat.Empty())
                    {
                        BitmapSource image = mat.ToBitmapSource();
                        image.Freeze(); // wichtig für UI-Thread
                        Dispatcher.BeginInvoke(new Action(() => this.updateImage(image)));

                    }
                    if (_isRecording && _writer != null)
                    {
                        _writer.Write(mat);
                    }
                    if (stopWatch.ElapsedMilliseconds > 1000)
                    {
                        counter = 0;
                        stopWatch.Restart();
                    }
                }
            }
            Debug.WriteLine("stopped {0}",System.Environment.CurrentManagedThreadId);
        });
        _thread.IsBackground = true;
        _thread.Start();
    }
    public delegate void updateEventDelegate(BitmapSource img);

    private bool _isRecording = false;
    private VideoWriter? _writer = null;
    private void ToggleRecording()
    {
        if (!_isRecording)
        {
            string filepath = videoRecordingFilePath;
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"recording_{timestamp}.mp4";
            //string fullfilepath = System.IO.Path.Combine(filepath, "logging");
            //string outputFile = System.IO.Path.Combine(fullfilepath, filename);
            string outputFile = filename;
            int fourcc = VideoWriter.FourCC('M', 'P', '4', 'V');
            double fps = camarafps;
            var frameSize = new OpenCvSharp.Size(_capture.FrameWidth, _capture.FrameHeight);

            _writer = new VideoWriter(outputFile, fourcc, fps, frameSize);

            if (!_writer.IsOpened())
            {
                MessageBox.Show("Failed to open video writer.");
                _writer.Dispose();
                _writer = null;
                return;
            }
            _isRecording = true;
        }
        else
        {
            _isRecording = false;
            _writer?.Release();
            _writer?.Dispose();
            _writer = null;
        }
    }
    private void updateImage(BitmapSource img)
    {
        webcamImage.Source = img;
    }

    private void StopView()
    {
        Debug.WriteLine("StopView() called");
        _cancellationTokenSource.Cancel();
        _thread.Join();
        _writer?.Release();
        _writer?.Dispose();
        _capture?.Release();
        _capture?.Dispose();
    }

    vmWorkspace _vmWorkSpace = null!;
    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vmWorkSpace = (vmWorkspace)e.NewValue;
        _vmWorkSpace.OnMainWindowClosed += _vmWorkSpace_OnMainWindowClosed;
        _vmWorkSpace.ToggleTheRecording += _vmWorkSpace_ToggleTheRecording;
    }

    private void _vmWorkSpace_ToggleTheRecording(object? sender, EventArgs e)
    {
        //ToggleRecording();
        Debug.WriteLine("Toggle Toggle");
    }

    private void _vmWorkSpace_OnMainWindowClosed(object? sender, EventArgs e)
    {
        StopView();
    }
    
}

