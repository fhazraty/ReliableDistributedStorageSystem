using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FullNode
{
    public class ConnectionManager
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public ConnectionManager(string IP, int port)
        {
            this.IP = IP;
            this.Port = port;
        }


    }
}
