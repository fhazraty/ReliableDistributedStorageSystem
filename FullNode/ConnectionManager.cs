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
        public bool ReceivingStopped { get; set; }
        public string SendIP { get; set; }
        public string ReceiveIP { get; set; }
        public int SendPort { get; set; }
        public int ReceivePort { get; set; }
        public List<List<long>> NetworkBandWidth { get; set; }
        public string StoragePath { get; set; }
        public ConnectionManager(string sendIp, int sendPort, string receiveIp, int receivePort, List<List<long>> networkBandWidth, string storagePath)
        {
            this.SendIP = sendIp;
            this.SendPort = sendPort;
            this.ReceiveIP = receiveIp;
            this.ReceivePort = receivePort;
            this.NetworkBandWidth = networkBandWidth;
            this.StoragePath = storagePath;
            ReceivingStopped = false;
        }
        public IBaseResult RegisterOnObserver(ObserverData observerData)
        {
            int retrycounter = 0;
            while (retrycounter < 10)
            {
                try
                {
                    lock (this)
                    {
                        ReceivingStopped = false;
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
                        client.Close();
                        client.Dispose();

                        return new RegisterSuccessResult()
                        {
                            ResultContainer = privateKey,
                            Successful = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
                Thread.Sleep(1000);
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
                ReceivingStopped = true;

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
        public void BlockReceiver()
        {
            //Listening ...
            TcpListener listener = new TcpListener(IPAddress.Parse(this.ReceiveIP), this.ReceivePort);
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

                    string pathToStore = StoragePath + receivedBlock.Content.Id.ToString();
                    if (!Directory.Exists(pathToStore))
                    {
                        Directory.CreateDirectory(pathToStore);
                    }
                    string fullPath = pathToStore +@"\" + receivedBlock.Content.SequenceNumber;

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
