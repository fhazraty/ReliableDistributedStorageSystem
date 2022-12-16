using MessagePack;
using Model;
using Utilities;

namespace FullNode
{
    public class Node : IDisposable
    {
        /// <summary>
        /// Creating a new full node
        /// </summary>
        /// <param name="nodeId">Id of fullnode</param>
        /// <param name="observerData">Observer data include ip address, port and etc...</param>
        /// <param name="sendIp">The ip address of full node to send data</param>
        /// <param name="sendPort">The port address of full node to send data</param>
        /// <param name="receiveIp">The ip address of full node to receive data</param>
        /// <param name="receivePort">The port address of full node to receive data</param>
        /// <param name="networkBandWidth">The network bandwidth array to simulate the network delay</param>
        /// <param name="mainPath">The storage path to save all data</param>
        /// <param name="sleepRetryObserver">Sleep between retry connecting to observer node</param>
        /// <param name="numberOfRetryObserver">The number of retries to connect to observer</param>
        /// <param name="randomizeRangeSleep">The randomize parameter of sleeping by multiply a random number between 0 and this number to sleeptime parameter for connecting to observer</param>
        /// <param name="sleepRetrySendFile">Sleep between retry sending files</param>
        /// <param name="numberOfRetrySendFile">The number of retries to send files</param>
        /// <param name="randomizeRangeSleepSendBlock">The randomize parameter of sleeping by multiply a random number between 0 and this number to sleeptime parameter for sending files</param>
        public Node
            (Guid nodeId,
            ObserverData observerData, 
            string sendIp, 
            int sendPort, 
            string receiveIp, 
            int receivePort,
            List<SpeedLine> networkBandWidth,
            string mainPath,
            int sleepRetryObserver,
            int numberOfRetryObserver,
            int randomizeRangeSleep,
            int sleepRetrySendFile,
            int numberOfRetrySendFile,
            int randomizeRangeSleepSendBlock)
        {
            this.Id = nodeId;

            this.MainPath = mainPath;

            //Define the storage path of full node which is c:\Miners\IdOfFullNode\
            this.StoragePath = MainPath + Id.ToString() + @"\";

            //Bulding this storage path if not exists
            BuildPathIfNotExists();

            this.ObserverData = observerData;

            //Initialize a new connection manager
            this.ConnectionManager = new ConnectionManager(sendIp, sendPort, receiveIp, receivePort, networkBandWidth, this.Id);
            
            //Registering this new node on observer
            var res = this.ConnectionManager.RegisterOnObserver(this.ObserverData, numberOfRetryObserver, sleepRetryObserver, randomizeRangeSleep);

            //If registering full node on observer is successful or not
            if (res.Successful)
            {
                // If yes, the private key is stored on node
                this.PrivateKey = ((RegisterSuccessResult)res).ResultContainer;
            }
            else
            {
                // There is a problem in connecting to observer and the operation must be stopped
                throw ((ErrorResult)res).ResultContainer;
            }

            this.TransactionManager = 
                new TransactionManager(
                    this.Id,
                    this.ConnectionManager,
                    this.StoragePath,
                    observerData,
                    sleepRetryObserver,
                    numberOfRetryObserver,
                    randomizeRangeSleep,
                    sleepRetrySendFile,
                    numberOfRetrySendFile,
                    randomizeRangeSleepSendBlock,
                    this.PrivateKey,
                    networkBandWidth);

            //Thread for receiving data from other nodes
            this.BlockReceiverThread = new Thread(new ThreadStart(this.TransactionManager.BlockReceiver));
            this.BlockReceiverThread.Start();

            //Thread for Synchronizing data between nodes
            this.BlockSyncThread = new Thread(new ThreadStart(this.TransactionManager.BlockSync));
            this.BlockSyncThread.Start();
        }
        public Thread BlockReceiverThread { get; set; }
        public Thread BlockSyncThread { get; set; }
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

            var chunks = file.Split((int)block.Header.BlockMaxSize).ToArray();

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
            BuildPathIfNotExists();

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
            this.TransactionManager.SyncingStopped = true;
            this.BlockReceiverThread.Abort();
        }
        public ConnectionManager ConnectionManager { get; set; }
        public TransactionManager TransactionManager { get; set; }
    }
}
