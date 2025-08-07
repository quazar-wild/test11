using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using OCTGui.ViewModels;

namespace OCTGui
{
    /// <summary>
    /// Interaction logic for ErrrorBox.xaml
    /// </summary>
    public partial class ErrorBox : Window
    {
        public ErrorBox( )
        {
            InitializeComponent();
            this.DataContextChanged += (_, e) =>
            {
                if (e.OldValue is vmErrorBox oldVm)
                    oldVm.RequestClose -= HandleRequestClose;

                if (e.NewValue is vmErrorBox newVm)
                    newVm.RequestClose += HandleRequestClose;
            };

        }

        private void HandleRequestClose(object? sender, EventArgs e)
        {
            this.DialogResult = true;
        }



        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }
    }
}
