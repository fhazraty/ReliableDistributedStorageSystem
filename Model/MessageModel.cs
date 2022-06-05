using MessagePack;

namespace Model
{
    [MessagePackObject]
    public class MessageModel
    {
        [Key(0)]
        public byte[] data;
        [Key(1)]
        public byte[] signature;
    }
}
