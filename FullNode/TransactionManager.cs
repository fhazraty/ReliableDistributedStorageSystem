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
    public class TransactionManager
    {
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
        public bool ReceivingStopped { get; set; }
        public string StoragePath { get; set; }
        public FullNodesData FullNodesData { get; set; }
        public ConnectionManager ConnectionManager { get; set; }
        public TransactionManager(
            ConnectionManager connectionManager, 
            string storagePath, 
            ObserverData observerData, 
            int sleepRetryObserver, 
            int numberOfRetryObserver, 
            int randomizeRangeSleep)
        {
            this.ConnectionManager = connectionManager;
            ReceivingStopped = false;
            this.StoragePath = storagePath;
            UpdateFullNodesData(observerData, sleepRetryObserver, numberOfRetryObserver, randomizeRangeSleep);
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
                        int retrier = 0;

                        while (retrier < numberOfRetrySendFile)
                        {
                            try
                            {
                                byte[] bytesToSend = MessagePackSerializer.Serialize(blocks[j]);

                                //Prepare TCP/IP Connection
                                TcpClient client = new TcpClient(fullNode.ReceiveIP, fullNode.ReceivePort);
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
                            Thread.Sleep(sleepRetrySendFile*r.Next(randomizeRangeSleep));
                        }
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

        public bool CheckTheHashIsValid(Block newblock)
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
                var currentBlock = GetCurrentBlock(newblock.Content.Id);
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

                    Block receivedBlock = MessagePackSerializer.Deserialize<Block>(msbyte);

                    if (!CheckTheHashIsValid(receivedBlock))
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
    }
}
