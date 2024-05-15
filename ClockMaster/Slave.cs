using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockMaster
{
    public class Slave
    {
        public string ID { get; private set; }
        public string PortName { get; private set; }

        public Slave(string id, string portName)
        {
            ID = id;
            PortName = portName;
        }
    }
}
