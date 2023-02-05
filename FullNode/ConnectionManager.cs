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
        public Guid Id { get; set; }
        public string SendIP { get; set; }
        public string ReceiveIP { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        public List<SpeedLine> NetworkBandWidth { get; set; }
        
        public ConnectionManager(string sendIp, int sendPort, string receiveIp, int receivePort, List<SpeedLine> networkBandWidth,Guid id)
        {
            this.SendIP = sendIp;
            this.SendPort = sendPort;
            this.ReceiveIP = receiveIp;
            this.ReceivePort = receivePort;
            this.NetworkBandWidth = networkBandWidth;
            this.Id = id;
        }
        public IBaseResult RegisterOnObserver(ObserverData observerData,int retryToConnect,int sleepTimer,int randomizeTimer)
        {
            int retrycounter = 0;
            while (retrycounter < retryToConnect)
            {
                try
                {
                    var cHelper = new CryptographyHelper();
                    byte[] publicKey;
                    byte[] privateKey;
                    cHelper.GenerateKeyRSA(out privateKey, out publicKey);

                    byte[] bytesToSend = MessagePackSerializer.Serialize(new FullNodesRecord()
                    {
                        SendIP = this.SendIP,
                        ReceiveIP = this.ReceiveIP,
                        ReceivePort = this.ReceivePort,
                        SendPort = this.SendPort,
                        PublicKey = publicKey,
                        Id = this.Id
                    });

                    TcpClient client = new TcpClient(observerData.AddIp, observerData.AddPort);
                    NetworkStream nwStream = client.GetStream();

                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    nwStream.Close();
                    nwStream.Dispose();
                    client.Close();
                    client.Dispose();

                    return new RegisterSuccessResult()
                    {
                        ResultContainer = privateKey,
                        Successful = true
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on register : " + ex.Message);
                }
                
                Random rnd = new Random();

                Thread.Sleep(sleepTimer * rnd.Next(0,randomizeTimer));

                retrycounter++;
            }
            return new ErrorResult()
            {
                Successful = false,
                ResultContainer = new Exception("retry threashold has been met!")
            };
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
