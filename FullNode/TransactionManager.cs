using MessagePack;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace FullNode
{
    public class TransactionManager
    {
        public bool ReceivingStopped { get; set; }
        public bool SyncingStopped { get; set; }
        public string StoragePath { get; set; }  // c:\Miners\IdOfFullNode\
        public FullNodesData FullNodesData { get; set; }
        public ConnectionManager ConnectionManager { get; set; }
        public ObserverData ObserverData { get; set; }
        public int SleepRetryObserver { get; set; }
        public int NumberOfRetryObserver { get; set; }
        public int RandomizeRangeSleep { get; set; }
        public byte[] PrivateKey { get; set; }
        public int Index { get; set; }
        public List<List<long>> NetworkBandWidth { get; set; }
        public TransactionManager(
            ConnectionManager connectionManager, 
            string storagePath, // c:\Miners\IdOfFullNode\
            ObserverData observerData, 
            int sleepRetryObserver, 
            int numberOfRetryObserver, 
            int randomizeRangeSleep,
            byte[] privateKey,
            int index,
            List<List<long>> networkBandWidth)
        {
            this.FullNodesData = new FullNodesData();
            this.ConnectionManager = connectionManager;
            ReceivingStopped = false;
            SyncingStopped = false;
            this.StoragePath = storagePath;  // c:\Miners\IdOfFullNode\
            this.ObserverData = observerData;
            this.SleepRetryObserver = sleepRetryObserver;
            this.NumberOfRetryObserver = numberOfRetryObserver;
            this.RandomizeRangeSleep = randomizeRangeSleep;
            this.PrivateKey = privateKey;
            this.Index = index;
            this.NetworkBandWidth = networkBandWidth;
            UpdateFullNodesData(this.ObserverData, this.SleepRetryObserver, this.NumberOfRetryObserver, this.RandomizeRangeSleep);
        }
        public IBaseResult UpdateFullNodesData(ObserverData observerData, int sleepRetryObserver, int numberOfRetryObserver,int randomizeRangeSleep)
        {
            var retryCounter = 0;
            while (retryCounter < numberOfRetryObserver)
            {
                try
                {
                    //Prepare TCP/IP Connection
                    TcpClient client = new TcpClient(observerData.BroadCastIp, observerData.BroadCastPort);
                    NetworkStream nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    //Read bytes ... 
                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);
                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    this.FullNodesData = MessagePackSerializer.Deserialize<FullNodesData>(ms.ToArray());

                    nwStream.Close();
                    nwStream.Dispose();

                    return new Result()
                    {
                        Successful = true
                    };
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    Console.WriteLine("here 1 : UpdateFullNodesData problem" + msg);
                }
                retryCounter++;
                Random r = new Random();
                Thread.Sleep(sleepRetryObserver* r.Next(randomizeRangeSleep));
            }
            //Exception occured!
            return new ErrorResult()
            {
                Successful = false,
                ResultContainer = new Exception("cannot connect to observer...")
            };
        }

        private void SendBytes(byte[] bytesToSend,string iP,int port,int numberOfRetrySendFile,long speedBytePerSecond,int sleepRetrySendFile,int randomizeRangeSleep)
        {
            int retrier = 0;
            while (retrier < numberOfRetrySendFile)
            {
                try
                {
                    //Prepare TCP/IP Connection
                    TcpClient client = new TcpClient(iP, port);
                    NetworkStream nwStream = client.GetStream();

                    long iterationCount = bytesToSend.Length / speedBytePerSecond;
                    if (bytesToSend.Length % speedBytePerSecond != 0)
                    {
                        iterationCount++;
                    }

                    for (int i = 0; i < (int)iterationCount; i++)
                    {
                        DateTime startSendingBuffer = DateTime.Now;

                        if (((i + 1) * speedBytePerSecond) < bytesToSend.Length)
                        {
                            nwStream.Write(bytesToSend, (int)(i * speedBytePerSecond), (int)speedBytePerSecond);
                        }
                        else
                        {
                            nwStream.Write(bytesToSend, (int)(i * speedBytePerSecond), (int)(bytesToSend.Length - (i * speedBytePerSecond)));
                        }
                        DateTime endSendingBuffer = DateTime.Now;

                        var timeTookToSend = endSendingBuffer.Subtract(startSendingBuffer).TotalMilliseconds;
                        if (timeTookToSend < 1000)
                        {
                            Thread.Sleep(1000 - (int)timeTookToSend);
                        }
                    }

                    nwStream.Close();
                    nwStream.Dispose();
                    client.Close();
                    client.Dispose();
                    retrier = numberOfRetrySendFile;
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    Console.WriteLine("here 2 : PropagateBlocks problem" + msg);
                }
                retrier++;
                Random r = new Random();
                Thread.Sleep(sleepRetrySendFile * r.Next(randomizeRangeSleep));
            }
        }

        public IBaseResult PropagateBlocks(
            List<Block> blocks, 
            ObserverData observerData,
            List<List<long>> networkBandWidth,
            int index,
            int sleepRetryObserver, 
            int numberOfRetryObserver,
            int sleepRetrySendFile,
            int numberOfRetrySendFile,
            int randomizeRangeSleep)
        {
            try
            {
                UpdateFullNodesData(observerData, sleepRetryObserver, numberOfRetryObserver, randomizeRangeSleep);
                int k = 0;
                foreach (var fullNode in FullNodesData.fullNodesRecords)
                {
                    if(index == k)
                    {
                        k++;
                        continue;
                    }
                    long speedBytePerSecond = networkBandWidth[index][k];

                    for (int j = 0; j < blocks.Count; j++)
                    {
                        byte[] bytesToSend = MessagePackSerializer.Serialize(blocks[j]);

                        SendBytes(bytesToSend, fullNode.ReceiveIP, fullNode.ReceivePort, numberOfRetrySendFile, speedBytePerSecond, sleepRetrySendFile, randomizeRangeSleep);
                    }
                    k++;
                }

                return new SendFileMode()
                {
                    Successful = true
                };
            }
            catch (Exception ex)
            {
                //Exception occured!
                return new ErrorResult()
                {
                    Successful = false,
                    ResultContainer = ex
                };
            }
        }
        public bool CheckTheHashIsValid(Block newblock,bool reCheck)
        {
            try
            {
                if (newblock.Content.SequenceNumber == 0)
                {
                    if (newblock.Header.HashPreviousBlock == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }

                Block currentBlock = null;
                if (reCheck)
                {
                    currentBlock = GetPreviousBlock(newblock.Content.Id,newblock.Content.SequenceNumber);
                }
                else
                {
                    currentBlock = GetCurrentBlock(newblock.Content.Id);
                }
                    
                byte[] bytesOfBlock = MessagePackSerializer.Serialize(currentBlock);

                var fNodeData = this.FullNodesData.fullNodesRecords.FirstOrDefault(f => f.Id == newblock.Content.Id);

                if (fNodeData == null) return false;

                var cHelper = new CryptographyHelper();

                return cHelper.Verify(cHelper.GetHashSha256ToByte(bytesOfBlock, 0, bytesOfBlock.Length), newblock.Header.HashPreviousBlock, fNodeData.PublicKey); ;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public Block? GetCurrentBlock(Guid id)
        {
            var listOfFiles = Directory.GetFiles(StoragePath + id.ToString() + @"\").ToList();
            if (listOfFiles.Count == 0) return null;
            var latestFile = listOfFiles.OrderByDescending(l => l.ToString()).ToArray()[0];
            var latestBlockByteArray = File.ReadAllBytes(latestFile);
            return MessagePackSerializer.Deserialize<Block>(latestBlockByteArray.ToArray());
        }
        public Block? GetPreviousBlock(Guid id, long sequenceNumber)
        {
            if(sequenceNumber == 0)
            {
                return null;
            }
            var latestBlockByteArray = File.ReadAllBytes(StoragePath + id.ToString() + @"\" + (sequenceNumber-1).ToString());
            return MessagePackSerializer.Deserialize<Block>(latestBlockByteArray.ToArray());
        }
        public void BlockReceiver()
        {
            //Listening ...
            TcpListener listener = new TcpListener(IPAddress.Parse(this.ConnectionManager.ReceiveIP), this.ConnectionManager.ReceivePort);
            listener.Start();

            while (!ReceivingStopped)
            {
                try
                {
                    //Accept connection ...
                    TcpClient client = listener.AcceptTcpClient();

                    //Get the incoming data through a network stream
                    NetworkStream nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    //Read bytes ... 
                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);
                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    var msbyte = ms.ToArray();

                    ms.Close();
                    ms.Dispose();

                    Block receivedBlock = null;
                    BlockStorageStatusListMessage blockStorageStatusListMessage = null;

                    try
                    {
                        receivedBlock = MessagePackSerializer.Deserialize<Block>(msbyte);
                    }
                    catch{}

                    try
                    {
                        blockStorageStatusListMessage = MessagePackSerializer.Deserialize<BlockStorageStatusListMessage>(msbyte);
                    }
                    catch{}


                    if (receivedBlock != null)
                    { 
                        if (!CheckTheHashIsValid(receivedBlock,false))
                        {
                            continue;
                        }

                        string pathToStore = StoragePath + receivedBlock.Content.Id.ToString();
                        if (!Directory.Exists(pathToStore))
                        {
                            Directory.CreateDirectory(pathToStore);
                        }
                        string fullPath = pathToStore + @"\" + receivedBlock.Content.SequenceNumber;

                        try
                        {
                            File.Delete(fullPath);
                        }
                        catch (Exception ex)
                        {
                        }

                        //Storing data on disk ...
                        using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                        {
                            fs.Write(msbyte, 0, msbyte.Length);
                        }
                    }

                    if(blockStorageStatusListMessage != null)
                    {
                        if (!blockStorageStatusListMessage.RequestToSend)
                        {
                            var diff = CheckLocalStatusAndShowTheDifference(blockStorageStatusListMessage.BlockStorageStatusList);
                            //var diff = blockStorageStatusListMessage.BlockStorageStatusList;


                            if (diff.BlockStorageStatuses.Count != 0)
                            {
                                byte[] bytesOfblockStorageStatusList = MessagePackSerializer.Serialize(diff);

                                var cHelper = new CryptographyHelper();

                                var diffMessage = new BlockStorageStatusListMessage()
                                {
                                    BlockStorageStatusList = diff,
                                    HashSignature = cHelper.Sign(cHelper.GetHashSha256ToByte(bytesOfblockStorageStatusList, 0, bytesOfblockStorageStatusList.Length), PrivateKey),
                                    ReceiveIpAddress = this.ConnectionManager.ReceiveIP,
                                    ReceivePort = this.ConnectionManager.ReceivePort,
                                    RequestToSend = true
                                };

                                byte[] bytesOfBlockStorageStatusListMessage = MessagePackSerializer.Serialize(diffMessage);

                                SendBytes(
                                    bytesOfBlockStorageStatusListMessage,
                                    blockStorageStatusListMessage.ReceiveIpAddress,
                                    blockStorageStatusListMessage.ReceivePort,
                                    10000,
                                    10 * 1024 * 1024,
                                    50,
                                    50);
                            }
                        }
                        else
                        {
                            var blocks = new List<Block>();

                            foreach (var status in blockStorageStatusListMessage.BlockStorageStatusList.BlockStorageStatuses.OrderBy(s => s.Id).ThenBy(s => s.SequenceNumber).ToList())
                            {
                                var blockBytes = File.ReadAllBytes(StoragePath + status.Id.ToString() + @"\" + status.SequenceNumber.ToString());
                                blocks.Add(MessagePackSerializer.Deserialize<Block>(blockBytes.ToArray()));
                            }

                            for (int j = 0; j < blocks.Count; j++)
                            {
                                byte[] bytesToSend = MessagePackSerializer.Serialize(blocks[j]);

                                SendBytes(
                                    bytesToSend, 
                                    blockStorageStatusListMessage.ReceiveIpAddress, 
                                    blockStorageStatusListMessage.ReceivePort,
                                    10000,
                                    10 * 1024 * 1024,
                                    50,
                                    50);
                            }
                        }
                    }

                    //Closing connection ...
                    client.Close();
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    Console.WriteLine("here 5 : BlockReceiver problem" + msg);
                }
            }
        }

        public BlockStorageStatusList CheckLocalStatusAndShowTheDifference(BlockStorageStatusList blockStorageStatusList)
        {
            var current = GetCurrentLocalStatus();
            var diff = new BlockStorageStatusList();
            diff.BlockStorageStatuses = new List<BlockStorageStatus>();

            foreach (var nStatus in blockStorageStatusList.BlockStorageStatuses)
            {
                bool found = false;

                foreach (var cStatus in current)
                {
                    if(cStatus.Id == nStatus.Id && cStatus.SequenceNumber == nStatus.SequenceNumber)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    diff.BlockStorageStatuses.Add(nStatus);
                }
            }

            return diff;
        }
        public void ValidateCurrentBlocks()
        {
            var directories = Directory.GetDirectories(this.StoragePath);

            foreach (var dir in directories)
            {
                var subDirectories = dir.Split('\\', StringSplitOptions.RemoveEmptyEntries);

                Guid directoryId = Guid.Parse(subDirectories[subDirectories.Length - 1]);

                var record = FindId(directoryId);

                if (record == null)
                {
                    //if the id is deleted from observer then all data of that node must be deleted...
                    Directory.Delete(dir, true);
                    continue;
                }

                DirectoryInfo di = new DirectoryInfo(dir);
                
                FileSystemInfo[] files = di.GetFileSystemInfos();
                
                var orderedFiles = files.OrderBy(f => f.Name).ToList();

                bool foundError = false;

                foreach (var file in orderedFiles)
                {
                    if (foundError)
                    {
                        File.Delete(file.FullName);
                    }
                    else
                    {
                        var allReadBytes = File.ReadAllBytes(file.FullName);

                        var block = MessagePackSerializer.Deserialize<Block>(allReadBytes);

                        if (!CheckTheHashIsValid(block,true))
                        {
                            foundError = true;

                            File.Delete(file.FullName);
                        }
                    }
                }
            }
        }
        public FullNodesRecord? FindId(Guid id)
        {
            foreach (var record in FullNodesData.fullNodesRecords)
            {
                if(record.Id == id)
                {
                    return record;
                }
            }
            return null;
        }
        public List<BlockStorageStatus> GetCurrentLocalStatus()
        {
            var list = new List<BlockStorageStatus>();

            var pathList = Directory.GetFiles(this.StoragePath, "*", SearchOption.AllDirectories);
            foreach (var path in pathList)
            {
                var status = new BlockStorageStatus();
                status.Id = Guid.Parse(path.Split('\\')[3]);
                status.SequenceNumber = int.Parse(Path.GetFileNameWithoutExtension(path));
                list.Add(status);
            }

            return list;
        }
        public void SendCurrentStatusToSync(
            FullNodesRecord fnRecord, 
            List<BlockStorageStatus> statuses,
            List<List<long>> networkBandWidth,
            int index,
            int sleepRetrySendFile,
            int numberOfRetrySendFile,
            int randomizeRangeSleep)
        {
            int k = 0;
            foreach (var fullNode in FullNodesData.fullNodesRecords)
            {
                if (index == k)
                {
                    k++;
                    continue;
                }
                long speedBytePerSecond = networkBandWidth[index][k];

                var blockStorageStatusList = new BlockStorageStatusList()
                {
                    BlockStorageStatuses = statuses
                };

                byte[] bytesOfblockStorageStatusList = MessagePackSerializer.Serialize(blockStorageStatusList);

                var cHelper = new CryptographyHelper();

                var msg = new BlockStorageStatusListMessage()
                {
                    BlockStorageStatusList = blockStorageStatusList,
                    HashSignature = cHelper.Sign(cHelper.GetHashSha256ToByte(bytesOfblockStorageStatusList, 0, bytesOfblockStorageStatusList.Length), PrivateKey),
                    ReceiveIpAddress = this.ConnectionManager.ReceiveIP,
                    ReceivePort = this.ConnectionManager.ReceivePort,
                    RequestToSend = false
                };

                byte[] bytesOfBlockStorageStatusListMessage = MessagePackSerializer.Serialize(msg);

                SendBytes(
                    bytesOfBlockStorageStatusListMessage, 
                    fnRecord.ReceiveIP, 
                    fnRecord.ReceivePort, 
                    numberOfRetrySendFile, 
                    speedBytePerSecond, 
                    sleepRetrySendFile, 
                    randomizeRangeSleep);
            }
        }
        public void BlockSync()
        {
            Thread.Sleep(60000);

            while (!ReceivingStopped)
            {
                UpdateFullNodesData(this.ObserverData, this.SleepRetryObserver, this.NumberOfRetryObserver, this.RandomizeRangeSleep);

                ValidateCurrentBlocks();

                var status = GetCurrentLocalStatus();

                UpdateFullNodesData(this.ObserverData, this.SleepRetryObserver, this.NumberOfRetryObserver, this.RandomizeRangeSleep);

                var fnListRecords = FullNodesData.fullNodesRecords.ToList();

                foreach (var fnRecord in fnListRecords)
                {
                    SendCurrentStatusToSync(fnRecord, status,this.NetworkBandWidth, this.Index, this.SleepRetryObserver, this.NumberOfRetryObserver, this.RandomizeRangeSleep);
                    
                    Thread.Sleep(60000);
                }
            }
        }
    }
}
