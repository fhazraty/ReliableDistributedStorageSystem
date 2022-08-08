using MessagePack;

namespace Model
{
    [MessagePackObject]
    public class FullNodesData
    {
        public FullNodesData()
        {
            fullNodesRecords = new List<FullNodesRecord>();
        }
        [Key(0)]
        public List<FullNodesRecord> fullNodesRecords { get; set; }
    }
}
