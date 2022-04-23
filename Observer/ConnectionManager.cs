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
        public ConnectionManager(ObserverData oData)
        {
            this.ObserverData = oData;
            this.StopThreadRequested = false;
            FullNodesData = new FullNodesData();
            LoadAllDataFromDisk();
        }
        public string FileStoragePath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\P2PStorage.txt";
            }
        }
        public FullNodesData FullNodesData { get; set; }
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

                    FullNodesRecord fNodeRecord = MessagePackSerializer.Deserialize<FullNodesRecord>(ms.ToArray());

                    AddNewRecord(fNodeRecord);

                    client.Close();
                    client.Dispose();
                    listenerAdd.Stop();
                }
                catch (Exception ex)
                {

                    throw;
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

                    throw;
                }
            }
        }
        public void Dispose()
        {
            StopThreadRequested = true;
            this.listenerAdd.Stop();
            this.listenerRemove.Stop();
        }
    }
}
