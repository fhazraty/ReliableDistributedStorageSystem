using MessagePack;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace FullNode
{
    public class Node
    {
        public Node(ObserverData observerData, string sendIp, int sendPort, string receiveIp, int receivePort)
        {
            Id = Guid.NewGuid();
            MainPath = @"c:\Miners\";
            StoragePath = MainPath + Id.ToString() + @"\";
            BuildPathIfNotExists();
            this.ObserverData = observerData;
            this.ConnectionManager = new ConnectionManager(sendIp, sendPort, receiveIp, receivePort);
            var res = this.ConnectionManager.RegisterOnObserver(this.ObserverData);
            if (res.Successful)
            {
                this.PrivateKey = ((RegisterSuccessResult)res).ResultContainer;
            }
            else
            {
                throw ((ErrorResult)res).ResultContainer;
            }
        }
        private void BuildPathIfNotExists()
        {
            if (!Directory.Exists(MainPath))
            {
                Directory.CreateDirectory(MainPath);
            }
            if (!Directory.Exists(StoragePath))
            {
                Directory.CreateDirectory(StoragePath);
            }
        }
        public Guid Id { get; set; }
        public int Version 
        { 
            get
            {
                return (new Block()).Header.Version;
            } 
        }
        public string MainPath { get; set; }
        public string StoragePath { get; set; }
        private byte[] PrivateKey { get; set; }
        public ObserverData ObserverData { get; set; }
        private List<Block> BuildBlocks(byte[] file,string filename,Guid id, int version,long sequenceStart, byte[] hashPreviousBlock)
        {
            var block = new Block();

            byte[][] chunks = file.Select((value, index) => new { PairNum = Math.Floor(index / (double)block.Header.BlockMaxSize), value }).GroupBy(pair => pair.PairNum).Select(grp => grp.Select(g => g.value).ToArray()).ToArray();

            var blocks = new List<Block>();

            Block previousBlock = null;

            for (int i = 0; i < chunks.Length; i++)
            {
                var blockContent = new BlockContent()
                {
                    Data = chunks[i],
                    FileName = filename,
                    StartFile = (i == 0),
                    EndFile = (i == chunks.Length - 1),
                    SequenceNumber = sequenceStart,
                    Id = id
                };

                var cHelper = new CryptographyHelper();

                var blockHeader = new BlockHeader();

                if (i == 0)
                {
                    blockHeader = new BlockHeader(){
                        HashPreviousBlock = hashPreviousBlock
                    };
                }
                else
                {
                    byte[] bytesOfBlock = MessagePackSerializer.Serialize(previousBlock);

                    blockHeader = new BlockHeader()
                    {
                        HashPreviousBlock = cHelper.Sign(cHelper.GetHashSha256ToByte(bytesOfBlock, 0, bytesOfBlock.Length), PrivateKey)
                    };
                }

                var newBlock = new Block()
                {
                    Content = blockContent,
                    Header = blockHeader
                };

                blocks.Add(newBlock);

                previousBlock = newBlock;

                sequenceStart++;
            }

            return blocks;
        }
        public IBaseResult SaveFile(string filename,byte[] fileContent,long sequenceStart, byte[] hashPreviousBlock)
        {
            var blocks = BuildBlocks(fileContent,filename,this.Id,this.Version, sequenceStart, hashPreviousBlock);
            return TransactionManager.PropagateBlocks(blocks, this.ObserverData);
        }
        public ConnectionManager ConnectionManager { get; set; }
        public TransactionManager TransactionManager { get; set; }
    }
}
