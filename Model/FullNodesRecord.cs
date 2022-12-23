using MessagePack;

namespace Model
{ 
    /// <summary>
    /// The data which is stored on observer
    /// </summary>
    [MessagePackObject]
    public class FullNodesRecord
    {
        /// <summary>
        /// The IP which is used for sending data
        /// </summary>
        [Key(0)]
        public string SendIP { get; set; }

        /// <summary>
        /// The IP which is used for receiving data
        /// </summary>
        [Key(1)]
        public string ReceiveIP { get; set; }

        /// <summary>
        /// The port which is used for sending data
        /// </summary>
        [Key(2)]
        public int SendPort { get; set; }

        /// <summary>
        /// The port which is used for receive data
        /// </summary>
        [Key(3)]
        public int ReceivePort { get; set; }

        /// <summary>
        /// The GUID of fullnode
        /// </summary>
        [Key(4)]
        public Guid Id { get; set; }

        /// <summary>
        /// The public key of the node which is needed to broadcast
        /// </summary>
        [Key(5)]
        public byte[] PublicKey { get; set; }
    }
}