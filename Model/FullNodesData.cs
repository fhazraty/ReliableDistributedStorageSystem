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
        [Key(3)]
        public List<FullNodesRecord> fullNodesRecords { get; set; }
    }
}
