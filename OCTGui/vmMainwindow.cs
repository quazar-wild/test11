using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace OCTGui
{
    class vmMainwindow : ObservableObject
    {
        private double _PulseLength;
        public double PulseLength { get => _PulseLength; set => SetProperty(ref _PulseLength, value); }


        private RelayCommand _UpdateCommand = null!;
        public ICommand UpdateCommand
        {
            get
            {
                if(_UpdateCommand == null)
                    _UpdateCommand = new RelayCommand(() => executeUpdateCommand(), () => canExecuteUpdateCommand());
                return _UpdateCommand;
            }
        }

        private bool canExecuteUpdateCommand()
        {
            return true;
        }

        private void executeUpdateCommand()
        {
            Console.Beep();
        }
    }
}
