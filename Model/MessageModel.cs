using MessagePack;

namespace Model
{
    /// <summary>
    /// This class used for test purposes
    /// </summary>
    [MessagePackObject]
    public class MessageModel
    {
        [Key(0)]
        public byte[] data;
        [Key(1)]
        public byte[] signature;
    }
}
