using MessagePack;

namespace Model
{
    /// <summary>
    /// Block of blockchain
    /// </summary>
    [MessagePackObject]
    public class Block
    {
        public Block()
        {
            Header = new BlockHeader();
            Content = new BlockContent();
        }
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
        /// The sequnce of blocks
        /// </summary>
        [Key(1)]
        public long SequenceNumber;

        /// <summary>
        /// Binary data files
        /// </summary>
        [Key(2)]
        public byte[] Data;

        /// <summary>
        /// File name
        /// </summary>
        [Key(3)]
        public string FileName;

        /// <summary>
        /// Is it the start of a new file ?
        /// </summary>
        [Key(4)]
        public bool StartFile;

        /// <summary>
        /// Is it the end of the file ?
        /// </summary>
        [Key(5)]
        public bool EndFile;
    }

    [MessagePackObject]
    public class BlockHeader
    {
        /// <summary>
        /// Signature of previous block
        /// </summary>
        [Key(0)]
        public byte[] HashPreviousBlock;
        [Key(1)]
        public long BlockMaxSize
        {
            get 
            {
                return 1024*1024*2; // 2Mb
            }
        }
        /// <summary>
        /// Blockchain version
        /// </summary>
        [Key(2)]
        public int Version
        {
            get
            {
                return 1;
            }
        }
    }
}
