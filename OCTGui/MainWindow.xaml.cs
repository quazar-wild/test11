using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.Serialization.DataContracts;
using System.Configuration;
using System.Windows;
using OCTGui.ViewModels;
using System.Security.Cryptography.X509Certificates;


namespace OCTGui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : System.Windows.Window
{
  
    public MainWindow()
    {
        
        InitializeComponent();
        this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        vmMainwindow vm = DataContext as vmMainwindow;
        if (vm != null) vm.OnMainWindowClosing(sender, e);
    }

    //private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
    //{
    //    AboutBox about = new AboutBox();

    //    about.Owner = this;
    //    about.ShowDialog();
    //}
}
