using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OCTGui
{
    public class LaserDiode
    {
        private IConnection _con;
        public LaserDiode(IConnection connection)
        {
            _con = connection;
        }
        // protocol related things?

        private bool _connectionStatus = true;
        public bool ConnectionStatus
        {
            get => _connectionStatus; set => _connectionStatus = value;
        }


        private DateTime _lastUpdateTime = DateTime.MinValue;
        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime; set => _lastUpdateTime = value;
        }


        public Thread _LaserDiodeThread = null;
        CancellationTokenSource _cts;
        private string data;
        private string _data;
        public string Data
        {
            get => _data; set => _data = value;
        }

        public void empty() { }

        public void Start()
        {

            _cts = new CancellationTokenSource();
            try
            {
                _con.Start();
            }
            catch (SocketException se)
            {
                ConnectionStatus = false;
                Debug.WriteLine($"Disconnected {se}");
                return;
            }
            _con.Send("p11");
            _con.Send("p22");
            LastUpdateTime = DateTime.Now;
            while ( !_cts.IsCancellationRequested)
            {
                _con.Send("g1");
                data = _con.Read();
                Thread.Sleep(500);
                if (data != string.Empty)
                {
                    Data = data;
                    Debug.WriteLine($"{Data}");
                    LastUpdateTime = DateTime.Now;
                }
            }
            _con.Send("p10");
        }
        public void Stop()
        {
            _cts?.Cancel();
            _LaserDiodeThread?.Join();
            _con?.Stop();
            LastUpdateTime = DateTime.MinValue;
            ConnectionStatus = true;
        }
    }
}
