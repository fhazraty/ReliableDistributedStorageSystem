using MessagePack;

namespace Model
{
    /// <summary>
    /// The status of each block
    /// </summary>
    [MessagePackObject]
    public class BlockStorageStatus
    {
        [Key(0)]
        public Guid Id { get; set; }
        [Key(1)]
        public int SequenceNumber { get; set; }
    }
    /// <summary>
    /// The list of status messages
    /// </summary>
    [MessagePackObject]
    public class BlockStorageStatusList
    {
        [Key(0)]
        public List<BlockStorageStatus> BlockStorageStatuses { get; set; }
    }
    /// <summary>
    /// The full message to send across network as status message
    /// </summary>
    [MessagePackObject]
    public class BlockStorageStatusListMessage
    {
        /// <summary>
        /// Sign of current message
        /// </summary>
        [Key(0)]
        public byte[] HashSignature;
        /// <summary>
        /// The current status of node
        /// </summary>
        [Key(1)]
        public BlockStorageStatusList BlockStorageStatusList { get; set; }
        /// <summary>
        /// receiver IP address
        /// </summary>
        [Key(2)]
        public string ReceiveIpAddress { get; set; }
        /// <summary>
        /// Receiver port number
        /// </summary>
        [Key(3)]
        public int ReceivePort { get; set; }
        /// <summary>
        /// Requested for send
        /// </summary>
        [Key(4)]
        public bool RequestToSend { get; set; }
    }
}
