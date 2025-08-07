using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCTGui
{
    public interface IConnection
    {
        void Start();
        void Stop();
        string Read();
        void Send(string message);
        void SetDTR(bool bState);
        void SetRTS(bool bState);
    }

}
