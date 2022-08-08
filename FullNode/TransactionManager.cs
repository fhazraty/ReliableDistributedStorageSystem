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
        public IBaseResult PropagateBlocks(List<Block> blocks, ObserverData observerData)
        {
            try
            {
                UpdateFullNodesData(observerData);

                foreach (var fullNode in FullNodesData.fullNodesRecords)
                {
                    //fullNode.ReceivePort
                    //fullNode.ReceiveIP
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
