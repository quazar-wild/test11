using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static OCTGui.ViewModels.vmWorkspace;

namespace OCTGui.Transport
{
    public class TcpTransportDev(string ip, int port) : IConnection
    {

        /// for GPIO demo
        public void SetDTR(bool bState) { }
        public void SetRTS(bool bState) { }
        /// </summary>

        private string ip = ip;
        private int port = port;

        private TcpClient client;
        private NetworkStream stream;

        public void Start()
        {
            client = new TcpClient();
            IAsyncResult result = client.BeginConnect(ip, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            if (!success)
            {
                Debug.WriteLine("Connection timed out");
                client.Close();
                throw new SocketException();
            }
            client.EndConnect(result);
            stream = client.GetStream();
        }
        public void Stop()
        {
            stream?.Close();
            client?.Close();
        }
        public string Read()
        {
            byte[] buffer = new byte[1024];
            if (stream.DataAvailable)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    return data.Trim();
                }
                return string.Empty;
            }
            else
            {
                Thread.Sleep(10);
                return string.Empty;
            }
        }
        public void Send(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }


    }
}
