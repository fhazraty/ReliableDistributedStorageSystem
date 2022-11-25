using MessagePack;
using Model;
using System.Net;
using System.Net.Sockets;

namespace Observer
{
    public class ConnectionManager: IDisposable
    {
        private readonly object fileLock = new object();
        private TcpListener listenerRemove;
        private TcpListener listenerAdd;
        private TcpListener listenerBroadCast;
        public bool StopThreadRequested { get; set; }
        public ObserverData ObserverData { get; set; }
        public FullNodesData FullNodesData { get; set; }
        public List<SpeedLine> NetworkBandWidth { get; set; }
        public string MainPath { get; set; }
        public string FileStoragePath { get; set; }
        public ConnectionManager(ObserverData oData, List<SpeedLine> networkBandWidth, string fileStoragePath)
        {
            this.ObserverData = oData;
            this.NetworkBandWidth = networkBandWidth;
            this.StopThreadRequested = false;
            this.MainPath = fileStoragePath;
            this.FileStoragePath = this.MainPath + "P2PStorage.txt";
            FullNodesData = new FullNodesData();
            LoadAllDataFromDisk();
        }
        private void BuildPathIfNotExists()
        {
            if (!Directory.Exists(MainPath))
            {
                Directory.CreateDirectory(MainPath);
            }
        }
        public bool AddNewRecord(FullNodesRecord record)
        {
            int counter = -1;
            bool found = false;
            foreach (var recordSearch in FullNodesData.fullNodesRecords)
            {
                counter++;
                if (recordSearch.SendIP == record.SendIP&&
                    recordSearch.SendPort == record.SendPort &&
                    recordSearch.ReceiveIP == record.ReceiveIP &&
                    recordSearch.ReceivePort == record.ReceivePort)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                RemoveRecord(record);
            }

            FullNodesData.fullNodesRecords.Add(record);
            
            this.PersistAllDataOnDisk();
            
            return true;
        }
        public bool RemoveRecord(FullNodesRecord record)
        {
            int counter = -1;
            bool found = false;
            foreach (var item in FullNodesData.fullNodesRecords)
            {
                counter++;
                if (item.SendIP == record.SendIP &&
                    item.SendPort == record.SendPort &&
                    item.ReceiveIP == record.ReceiveIP &&
                    item.ReceivePort == record.ReceivePort)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                FullNodesData.fullNodesRecords.RemoveAt(counter);

                this.PersistAllDataOnDisk();
            }
            return found;
        }
        public bool PersistAllDataOnDisk()
        {
            lock (fileLock)
            {
                try
                {
                    File.Delete(FileStoragePath);

                    byte[] bytesToWrite = MessagePackSerializer.Serialize(FullNodesData);

                    var fStream = new FileStream(FileStoragePath, FileMode.CreateNew, FileAccess.Write);

                    BinaryWriter bw = new BinaryWriter(fStream);

                    bw.Write(bytesToWrite);
                    bw.Close();
                    bw.Dispose();
                    fStream.Close();
                    fStream.Dispose();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }

            }
        }
        public bool LoadAllDataFromDisk()
        {
            lock (fileLock)
            {
                try
                {
                    var fStream = new FileStream(FileStoragePath, FileMode.Open, FileAccess.Read);

                    byte[] bytesToRead = File.ReadAllBytes(FileStoragePath);

                    FullNodesData = MessagePackSerializer.Deserialize<FullNodesData>(bytesToRead.ToArray());

                    fStream.Close();
                    fStream.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        public void ListenToAddIpPort()
        {
            while (!StopThreadRequested)
            {
                try
                {

                    listenerAdd = new TcpListener(IPAddress.Parse(ObserverData.AddIp), ObserverData.AddPort);
                    listenerAdd.Start();

                    TcpClient client = listenerAdd.AcceptTcpClient();

                    NetworkStream nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);
                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    var bytearray = ms.ToArray();
                    ms.Close();
                    ms.Dispose();
                    client.Close();
                    client.Dispose();
                    listenerAdd.Stop();

                    FullNodesRecord fNodeRecord = MessagePackSerializer.Deserialize<FullNodesRecord>(bytearray);

                    AddNewRecord(fNodeRecord);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }   
        }
        public void ListenToRemoveIpPort()
        {
            while (!StopThreadRequested)
            {
                try
                {
                    listenerRemove = new TcpListener(IPAddress.Parse(ObserverData.RemoveIp), ObserverData.RemovePort);
                    listenerRemove.Start();

                    TcpClient client = listenerRemove.AcceptTcpClient();

                    NetworkStream nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);
                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    FullNodesRecord fNodeRecord = MessagePackSerializer.Deserialize<FullNodesRecord>(ms.ToArray());

                    RemoveRecord(fNodeRecord);

                    client.Close();
                    client.Dispose();
                    listenerRemove.Stop();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }
        }
        public void ListenToBroadCastIpPort()
        {
            listenerBroadCast = new TcpListener(IPAddress.Parse(ObserverData.BroadCastIp), ObserverData.BroadCastPort);
            listenerBroadCast.Start();

            while (!StopThreadRequested)
            {
                try
                {
                    TcpClient client = listenerBroadCast.AcceptTcpClient();

                    IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    if(remoteIpEndPoint == null) { return ; }
                    
                    LoadAllDataFromDisk();

                    int counter = 0;
                    foreach (var fullNodeData in FullNodesData.fullNodesRecords)
                    {
                        //Find index of fullnode to adapt networkbandwith based on initialization
                        if(fullNodeData.SendIP == remoteIpEndPoint.Address.ToString())
                        {
                            break;
                        }
                        counter++;
                    }
                    long sendSpeedFromObserverToFullNode = 10*1024*1024;

                    
                    using (NetworkStream nwStream = client.GetStream())
                    {
                        //Serialize in binary format
                        byte[] bytesToSend = MessagePackSerializer.Serialize(FullNodesData);

                        long iterationCount = bytesToSend.Length / sendSpeedFromObserverToFullNode;
                        if (bytesToSend.Length % sendSpeedFromObserverToFullNode != 0)
                        {
                            iterationCount++;
                        }

                        for (int i = 0; i < iterationCount; i++)
                        {
                            DateTime startSendingBuffer = DateTime.Now;

                            if (((i + 1) * sendSpeedFromObserverToFullNode) < bytesToSend.Length)
                            {
                                nwStream.Write(bytesToSend, (int)(i * sendSpeedFromObserverToFullNode), (int)sendSpeedFromObserverToFullNode);
                            }
                            else
                            {
                                nwStream.Write(bytesToSend, (int)(i * sendSpeedFromObserverToFullNode), (int)(bytesToSend.Length - (i * sendSpeedFromObserverToFullNode)));
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
                    }
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    Console.WriteLine("here 4 : ListenToBroadCastIpPort problem" + msg);
                }
            }
        }
        public void Dispose()
        {
            StopThreadRequested = true;
            this.listenerAdd.Stop();
            this.listenerRemove.Stop();
            this.listenerBroadCast.Stop();
        }
    }
}
