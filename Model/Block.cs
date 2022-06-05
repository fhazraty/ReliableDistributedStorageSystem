using MessagePack;

namespace Model
{
    /// <summary>
    /// Block of blockchain
    /// </summary>
    [MessagePackObject]
    public class Block
    {
        /// <summary>
        /// The header of block
        /// </summary>
        [Key(0)]
        public BlockHeader Header { get; set; }
        /// <summary>
        /// Content of block
        /// </summary>
        [Key(1)]
        public BlockContent Content { get; set; }
    }

    [MessagePackObject]
    public class BlockContent
    {
        /// <summary>
        /// Sender Id which is binded to a specific certificate
        /// </summary>
        [Key(0)]
        public Guid Id;

        /// <summary>
        /// Blockchain version
        /// </summary>
        [Key(1)]
        public int version;

        /// <summary>
        /// The sequnce of blocks
        /// </summary>
        [Key(2)]
        public long SequenceNumber;

        /// <summary>
        /// Binary data files
        /// </summary>
        [Key(3)]
        public byte[] data;

        /// <summary>
        /// File name
        /// </summary>
        [Key(4)]
        public string fileName;

        /// <summary>
        /// Is it the start of a new file ?
        /// </summary>
        [Key(5)]
        public bool startFile;

        /// <summary>
        /// Is it the end of the file ?
        /// </summary>
        [Key(6)]
        public bool endFile;
    }

    [MessagePackObject]
    public class BlockHeader
    {
        /// <summary>
        /// Signature of current block
        /// </summary>
        [Key(0)]
        public byte[] signatureCurrentBlock;

        /// <summary>
        /// Signature of previous block
        /// </summary>
        [Key(1)]
        public byte[] signaturePreviousBlock;
    }
}
