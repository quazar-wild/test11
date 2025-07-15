using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows.Threading;


namespace OCTGui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window
{
    private VideoCapture _capture;
    private bool _isRunning = true;
    public MainWindow()
    {
        InitializeComponent();
        StartCamera();
    }

    private void StartCamera()
    {
        _capture = new VideoCapture(); // 0 = erste Kamera
        _capture.Open(0,VideoCaptureAPIs.DSHOW);

        Thread thread = new Thread(() =>
        {
            using (var mat = new Mat())
            {
                while (_isRunning)
                {
                    _capture.Read(mat);
                    if (!mat.Empty())
                    {
                        var image = mat.ToBitmapSource();
                        image.Freeze(); // wichtig für UI-Thread
                        Dispatcher.Invoke(() => webcamImage.Source = image);
                    }
                }
            }
        });
        thread.IsBackground = true;
        thread.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _isRunning = false;
        _capture?.Release();
        base.OnClosed(e);
    }

    private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        AboutBox about = new AboutBox();
        about.Owner = this;
        about.ShowDialog();
    }
}
