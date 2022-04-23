using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ObserverData
    {
        public string BroadCastIp { get; set; }
        public int BroadCastPort { get; set; }
        public string AddIp { get; set; }
        public int AddPort { get; set; }
        public string RemoveIp { get; set; }
        public int RemovePort { get; set; }
    }
}
