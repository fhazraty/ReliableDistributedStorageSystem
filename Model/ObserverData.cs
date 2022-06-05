using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ObserverData
    {
        public ObserverData()
        {

        }
        public ObserverData(string broadCastIp,int broadCastPort,string addIp,int addPort,string removeIp,int removePort)
        {
            this.BroadCastIp = broadCastIp;
            this.BroadCastPort = broadCastPort;
            this.AddIp = addIp;
            this.AddPort = addPort;
            this.RemoveIp = removeIp;
            this.RemovePort = removePort;
        }
        public string BroadCastIp { get; set; }
        public int BroadCastPort { get; set; }
        public string AddIp { get; set; }
        public int AddPort { get; set; }
        public string RemoveIp { get; set; }
        public int RemovePort { get; set; }
    }
}
