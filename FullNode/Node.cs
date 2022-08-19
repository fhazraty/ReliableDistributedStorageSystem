using MessagePack;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace FullNode
{
    public class Node : IDisposable
    {
        public Node(
            ObserverData observerData, 
            string sendIp, 
            int sendPort, 
            string receiveIp, 
            int receivePort,
            List<List<long>> networkBandWidth,
            int index,
            string mainPath,
            int sleepRetryObserver,
            int numberOfRetryObserver,
            int randomizeRangeSleep)
        {
            this.Id = Guid.NewGuid();
            this.Index = index;
            this.MainPath = mainPath;
            this.StoragePath = MainPath + Id.ToString() + @"\";
            BuildPathIfNotExists();
            this.ObserverData = observerData;
            this.ConnectionManager = new ConnectionManager(sendIp, sendPort, receiveIp, receivePort, networkBandWidth, this.Id);
            this.TransactionManager = new TransactionManager(
                this.ConnectionManager, 
                this.StoragePath, 
                observerData,
                sleepRetryObserver,
                numberOfRetryObserver,
                randomizeRangeSleep);
            var res = this.ConnectionManager.RegisterOnObserver(this.ObserverData);
            if (res.Successful)
            {
                this.PrivateKey = ((RegisterSuccessResult)res).ResultContainer;
            }
            else
            {
                throw ((ErrorResult)res).ResultContainer;
            }

            this.BlockReceiverThread = new Thread(new ThreadStart(this.TransactionManager.BlockReceiver));
            this.BlockReceiverThread.Start();
        }
        public Thread BlockReceiverThread { get; set; }
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
            if (!Directory.Exists(StoragePath + Id.ToString() + @"\"))
            {
                Directory.CreateDirectory(StoragePath + Id.ToString() + @"\");
            }
        }
        public Guid Id { get; set; }
        public int Index { get; set; }
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
        private byte[] GetHashOfBlock(Block block)
        {
            if (block == null) return null;
            byte[] bytesOfBlock = MessagePackSerializer.Serialize(block);
            var cHelper = new CryptographyHelper();
            return cHelper.Sign(cHelper.GetHashSha256ToByte(bytesOfBlock, 0, bytesOfBlock.Length), PrivateKey);
        }
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

                var blockHeader = new BlockHeader();

                if (i == 0)
                {
                    blockHeader = new BlockHeader(){
                        HashPreviousBlock = hashPreviousBlock
                    };
                }
                else
                {
                    blockHeader = new BlockHeader()
                    {
                        HashPreviousBlock = GetHashOfBlock(previousBlock)
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
        public Block? GetCurrentBlock()
        {
            var listOfFiles = Directory.GetFiles(StoragePath + Id.ToString() + @"\").ToList();
            if (listOfFiles.Count == 0) return null;
            var latestFile = listOfFiles.OrderByDescending(l => l.ToString()).ToArray()[0];
            var latestBlockByteArray = File.ReadAllBytes(StoragePath + latestFile);
            return MessagePackSerializer.Deserialize<Block>(latestBlockByteArray.ToArray());
        }
        private IBaseResult SaveFile(
            string filename,
            byte[] fileContent,
            long sequenceStart, 
            byte[] hashPreviousBlock,
            int sleepRetryObserver,
            int numberOfRetryObserver,
            int sleepRetrySendFile,
            int numberOfRetrySendFile,
            int randomizeRangeSleep)
        {
            var blocks = BuildBlocks(fileContent,filename,this.Id,this.Version, sequenceStart, hashPreviousBlock);

            foreach (var block in blocks)
            {
                string pathToStore = StoragePath + Id.ToString() + @"\";
                if (!Directory.Exists(pathToStore))
                {
                    Directory.CreateDirectory(pathToStore);
                }
                string fullPath = pathToStore + @"\" + block.Content.SequenceNumber;

                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                }

                var msbyte = MessagePackSerializer.Serialize<Block>(block);

                //Storing data on local disk ...
                using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                {
                    fs.Write(msbyte, 0, msbyte.Length);
                }
            }

            return TransactionManager.PropagateBlocks(
                blocks, 
                this.ObserverData, 
                ConnectionManager.NetworkBandWidth, 
                this.Index, 
                sleepRetryObserver,
                numberOfRetryObserver,
                sleepRetrySendFile,
                numberOfRetrySendFile,
                randomizeRangeSleep);
        }
        public IBaseResult SaveFile(
            string filename, 
            byte[] fileContent, 
            int sleepRetryObserver,
            int numberOfRetryObserver,
            int sleepRetrySendFile,
            int numberOfRetrySendFile,
            int randomizeRangeSleep)
        {
            var currentBlock = GetCurrentBlock();

            long sequenceStart = 0;
            if(currentBlock != null)  sequenceStart = currentBlock.Content.SequenceNumber + 1; 

            return SaveFile(
                filename, 
                fileContent,
                sequenceStart,
                GetHashOfBlock(currentBlock),
                sleepRetryObserver,
                numberOfRetryObserver,
                sleepRetrySendFile,
                numberOfRetrySendFile,
                randomizeRangeSleep);
        }
        public void Dispose()
        {
            this.TransactionManager.ReceivingStopped = true;
            this.BlockReceiverThread.Abort();
        }
        public ConnectionManager ConnectionManager { get; set; }
        public TransactionManager TransactionManager { get; set; }
    }
}
