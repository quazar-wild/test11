using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Provider;

namespace OCTGui.Devices
{
    public class OCTProZ
    {
        private IConnection _con;
        public OCTProZ(IConnection connection)
        {
            _con = connection;
        }
        // protocol related things?


        private string _octSignal = null!;
        public string OctSignal
        {
            get => _octSignal; set => _octSignal = value;
        }
        private DateTime _lastReceivedMessageTime = DateTime.MinValue;
        public DateTime LastReceivedMessageTime
        {
            get => _lastReceivedMessageTime; set => _lastReceivedMessageTime = value;
        }

        private DateTime _timeOfConnection = DateTime.MinValue;
        public DateTime TimeOfConnection
        {
            get => _timeOfConnection; set => _timeOfConnection = value;
        }

        private bool _connectionStatus = true;
        public bool ConnectionStatus
        {
            get => _connectionStatus; set => _connectionStatus = value;
        }

        public Thread _OCTThread = null;
        CancellationTokenSource _cts;
        private string data;
        private string _data;
        public string Data
        {
            get => _data; set => _data = value;
        }
        public void Start()
        {

            OctSignal = null!;
            ConnectionStatus = true;
            _cts = new CancellationTokenSource();
            try
            {
                _con.Start();
            }
            catch(SocketException se)
            {
                ConnectionStatus = false;
                Debug.WriteLine("no connection");
                return;
            }
            _con.Send("start_oct");
            TimeOfConnection = DateTime.Now;
            while (!_cts.IsCancellationRequested)
            {
                data = _con.Read();
                if (data != string.Empty)
                {
                    Data = data;
                    Debug.WriteLine($"{Data}");
                    if (Data == "<ID01><RPA><E>")
                        OctSignal = "Ready";
                    else if (Data == "<ID02><RPB><E>")
                        OctSignal = "notReady";
                    else
                        OctSignal = "iDontKnow";
                    LastReceivedMessageTime = DateTime.Now;
                    Debug.WriteLine(LastReceivedMessageTime);
                }
            }
            _con.Send("stop_oct");
        }
        public void Stop()
        {
            _cts?.Cancel();
            _OCTThread?.Join();
            Debug.WriteLine("Thread Joined");
            _con?.Stop();
            LastReceivedMessageTime = DateTime.MinValue;
            TimeOfConnection = DateTime.MinValue;
            OctSignal = null!;
            ConnectionStatus = true;
        }
    }
}
