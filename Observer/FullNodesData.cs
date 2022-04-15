using System.Net;
using MessagePack;

namespace Observer
{
    [MessagePackObject]
    public class FullNodesRecord
    {
        [Key(0)]
        public string IPAddress { get; set; }
        [Key(1)]
        public int PortSend { get; set; }
        [Key(2)]
        public int PortReceive { get; set; }
    }
    [MessagePackObject]
    public class FullNodesData
    {
        public FullNodesData()
        {
            fullNodesRecords = new List<FullNodesRecord>();
        }
        [Key(3)]
        public List<FullNodesRecord> fullNodesRecords { get; set; }
    }
}
