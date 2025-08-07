using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCTGui
{
    class GPIO_Demo
    {
        private IConnection _con;
        public GPIO_Demo(IConnection connection) 
        {
            _con = connection;
        }
        
        private bool _active = false;

        public void start()
        {
            _con.Start();
        }

        public void send(bool canRunLaser) 
        {
            if (Properties.Settings.Default.GPIO_Demo.Split(":")[0] == "TCP")
            {
                if (!canRunLaser)
                    _con.Send("8,RA");
                if (canRunLaser)
                {
                    _con.Send("17,CX");
                }

            }
            if (Properties.Settings.Default.GPIO_Demo.Split(':')[0] == "COM")
            {
                    _con.SetDTR(canRunLaser);
                    _con.SetRTS(canRunLaser);
            }
        }

        public void stop()
        {
            if (_active)
            {
                if (Properties.Settings.Default.GPIO_Demo.Split(":")[0] == "TCP")
                {
                    _con.Send("8,RA");
                }
                if (Properties.Settings.Default.GPIO_Demo.Split(':')[0] == "COM")
                {
                    _con.SetDTR(false);
                    _con.SetRTS(false);
                }
                _active = false;
            }
            _con?.Stop();
        }

    }
}
