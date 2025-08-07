using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OCTGui.Transport
{
    public class COMTransportDev(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : IConnection
    {
        private string portName = portName;
        private int baudRate = baudRate;
        private Parity parity = parity;
        private int dataBits = dataBits;
        private StopBits stopBits = stopBits;


        private SerialPort port;
        public void SetDTR(bool bState)
        {
            port.DtrEnable = bState;
        }
        public void SetRTS(bool bState)
        {
            port.RtsEnable = bState;
        }

        public void Send(string message)
        {
            port.Write(message);
        }

        public void Start()
        {
            if (!SerialPort.GetPortNames().Contains(portName))
            {
                Debug.WriteLine("Port does not exist");
                throw new SocketException(); 
            }
            port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            try
            {
                port.Open();
            }
            catch (Exception ex) 
            { 
                throw new SocketException();
            }
        }
        public string Read()
        {
            if (port.BytesToRead > 0)
            {
                int available = port.BytesToRead;
                byte[] buffer = new byte[available];
                int bytesRead = port.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0) 
                {
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    return data;
                }
                return string.Empty;
            }
            else
            {
                Thread.Sleep(10);
                return string.Empty;
            }
        }
        public void Stop()
        {
            if (port != null)
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
                port?.Dispose();
            }
            Debug.WriteLine("ports are closed");
        }
    }
}
