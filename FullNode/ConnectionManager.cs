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
    public class ConnectionManager
    {
        public string SendIP { get; set; }
        public string ReceiveIP { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        public ConnectionManager(string sendIp, int sendPort, string receiveIp, int receivePort)
        {
            this.SendIP = sendIp;
            this.SendPort = sendPort;
            this.ReceiveIP = receiveIp;
            this.ReceivePort = receivePort;
        }
        public IBaseResult RegisterOnObserver(ObserverData observerData)
        {
            try
            {
                var cHelper = new CryptographyHelper();
                byte[] publicKey;
                byte[] privateKey;
                cHelper.GenerateKey(out privateKey, out publicKey);

                byte[] bytesToSend = MessagePackSerializer.Serialize(new FullNodesRecord()
                {
                    SendIP = this.SendIP,
                    ReceiveIP = this.ReceiveIP,
                    ReceivePort = this.ReceivePort,
                    SendPort = this.SendPort,
                    PublicKey = publicKey
                });

                TcpClient client = new TcpClient(observerData.AddIp, observerData.AddPort);
                NetworkStream nwStream = client.GetStream();

                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                nwStream.Close();
                nwStream.Dispose();

                return new RegisterSuccessResult()
                {
                    ResultContainer = privateKey,
                    Successful = true
                };
            }
            catch (Exception ex)
            {
                return new ErrorResult()
                {
                    Successful = false,
                    ResultContainer = ex
                };
            }
        }
        public bool RemoveFromObserver(ObserverData observerData)
        {
            try
            {
                byte[] bytesToSend = MessagePackSerializer.Serialize(new FullNodesRecord()
                {
                    SendIP = this.SendIP,
                    ReceiveIP = this.ReceiveIP,
                    ReceivePort = this.ReceivePort,
                    SendPort = this.SendPort
                });

                TcpClient client = new TcpClient(observerData.RemoveIp, observerData.RemovePort);
                NetworkStream nwStream = client.GetStream();

                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                nwStream.Close();
                nwStream.Dispose();

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
