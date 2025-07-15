using System.Configuration;
using System.Data;
using System.Windows;

namespace OCTGui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var vm = new vmMainwindow();
        var window = new MainWindow();
        window.DataContext = vm;

        window.Show();
    }
}

