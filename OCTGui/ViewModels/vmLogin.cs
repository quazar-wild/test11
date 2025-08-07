using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Security;
using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace OCTGui.ViewModels
{
    public class vmLogin : ObservableObject
    {
        public vmLogin()
        {
            
        }

        private string _userName;
        public string UserName 
        {
            get => _userName; set => SetProperty(ref _userName, value); 
        }

        //public SecureString Password { private get; set; } //für die Sicherheit vllt

        public string Password { private get; set; }

        public bool CanLogIn()
        {
            //Properties.Settings.Default.user = "123";
            //Properties.Settings.Default.Save();
            return true;
            //if (Properties.Settings.Default.user == Password) return true;
            //else return false;
        }
       
    }
}
