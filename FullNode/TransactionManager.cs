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
        public IBaseResult UpdateFullNodesData(ObserverData observerData)
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
                //Exception occured!
                return new ErrorResult()
                {
                    Successful = false,
                    ResultContainer = ex
                };
            }

        }
        public IBaseResult PropagateBlocks(List<Block> blocks, ObserverData observerData,List<List<long>> networkBandWidth,int index)
        {
            try
            {
                UpdateFullNodesData(observerData);
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

                        while (retrier < 10)
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
                                retrier = 10;
                            }
                            catch (Exception ex)
                            {
                                string msg2 = ex.Message;
                            }
                            retrier++;
                            Thread.Sleep(1000);
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
