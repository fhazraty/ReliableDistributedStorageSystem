using MessagePack;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FullNode
{
    public class ConnectionManager
    {
        public string SendIP { get; set; }
        public string ReceiveIP { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        public ObserverData ObserverData { get; set; }
        public ConnectionManager(string sendIp, int sendPort, string receiveIp, int receivePort, ObserverData observerData)
        {
            this.SendIP = sendIp;
            this.SendPort = sendPort;
            this.ReceiveIP = receiveIp;
            this.ReceivePort = receivePort;
            this.ObserverData = observerData;
        }

        public bool RegisterOnObserver()
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

                TcpClient client = new TcpClient(ObserverData.AddIp, ObserverData.AddPort);
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

        public bool RemoveFromObserver()
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

                TcpClient client = new TcpClient(ObserverData.RemoveIp, ObserverData.RemovePort);
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
