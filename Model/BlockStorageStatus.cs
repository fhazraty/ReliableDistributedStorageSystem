using MessagePack;

namespace Model
{
    [MessagePackObject]
    public class BlockStorageStatus
    {
        [Key(0)]
        public Guid Id { get; set; }
        [Key(1)]
        public int SequenceNumber { get; set; }
    }
    [MessagePackObject]
    public class BlockStorageStatusList
    {
        [Key(0)]
        public List<BlockStorageStatus> BlockStorageStatuses { get; set; }
    }
    [MessagePackObject]
    public class BlockStorageStatusListMessage
    {
        [Key(0)]
        public byte[] HashSignature;
        [Key(1)]
        public BlockStorageStatusList BlockStorageStatusList { get; set; }
        [Key(2)]
        public string ReceiveIpAddress { get; set; }
        [Key(3)]
        public int ReceivePort { get; set; }
        [Key(4)]
        public bool RequestToSend { get; set; }
    }
}
