using MessagePack;

namespace Model
{ 
    [MessagePackObject]
    public class FullNodesRecord
    {
        [Key(0)]
        public string SendIP { get; set; }
        [Key(1)]
        public string ReceiveIP { get; set; }
        [Key(2)]
        public int SendPort { get; set; }
        [Key(3)]
        public int ReceivePort { get; set; }
    }
}