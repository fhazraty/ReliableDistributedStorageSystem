using MessagePack;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public FullNodesData FullNodesData { get; set; }
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
    }
}
