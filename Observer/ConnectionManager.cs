using MessagePack;
using Model;
using System.Net;
using System.Net.Sockets;

namespace Observer
{
    /// <summary>
    /// Responsible to manage connections of the observer
    /// </summary>
    public class ConnectionManager: IDisposable
    {
        public Guid Id { get; set; }
        // The mutex for store and retrieve data from a single text file
        private readonly object fileLock = new object();
        
        // Listener for removing nodes
        private TcpListener listenerRemove;

        // Listener for adding new nodes
        private TcpListener listenerAdd;

        // Listener for broadcasting nodes data across the network
        private TcpListener listenerBroadCast;

        // Controls the concurrent threads to stop when is requested
        public bool StopThreadRequested { get; set; }

        // Information about observer
        public ObserverData ObserverData { get; set; }

        // The list of fullnodes data
        public FullNodesData FullNodesData { get; set; }

        // The array list of Bandwidth in the network
        public List<SpeedLine> NetworkBandWidth { get; set; }

        // The address of data storage about nodes like ip addresses and public key certificates
        public string MainPath { get; set; }

        // The file name which stores data
        public string FileStoragePath { get; set; }
        /// <summary>
        /// Builds a connection manager for observer
        /// </summary>
        /// <param name="oData">Information about observer</param>
        /// <param name="networkBandWidth">Bandwidth of network</param>
        /// <param name="fileStoragePath">The address of data storage about nodes like ip addresses and public key certificates</param>
        public ConnectionManager(ObserverData oData, List<SpeedLine> networkBandWidth, string fileStoragePath, Guid id)
        {
            Id = id;

            this.ObserverData = oData;
            this.NetworkBandWidth = networkBandWidth;
            this.StopThreadRequested = false;
            this.MainPath = fileStoragePath;
            this.FileStoragePath = this.MainPath + "P2PStorage.txt";
            FullNodesData = new FullNodesData();

            //Load all previous data from disk
            LoadAllDataFromDisk();
        }
        /// <summary>
        /// Add new record of fullnode
        /// </summary>
        /// <param name="record">The data which must be added to list</param>
        /// <returns>The operation is successful or not</returns>
        public bool AddNewRecord(FullNodesRecord record)
        {
            try
            {
                // Try to find duplicated record
                bool found = false;
                foreach (var recordSearch in FullNodesData.fullNodesRecords)
                {
                    // If the duplicated record is founded
                    if (recordSearch.SendIP == record.SendIP &&
                        recordSearch.SendPort == record.SendPort &&
                        recordSearch.ReceiveIP == record.ReceiveIP &&
                        recordSearch.ReceivePort == record.ReceivePort)
                    {
                        found = true;
                        break;
                    }
                }

                // The duplicated record is deleted...
                if (found)
                {
                    RemoveRecord(record);
                }

                //Add new record which is not duplicate
                FullNodesData.fullNodesRecords.Add(record);

                //Save all data on the file
                PersistAllDataOnDisk();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Removes a fullnode record from list and then persist on disk
        /// </summary>
        /// <param name="record">The node which must be removed</param>
        /// <returns>The operation is successful or not</returns>
        public bool RemoveRecord(FullNodesRecord record)
        {
            int counter = -1;
            bool found = false;

            // Finding the index of fullnode in the list
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
                // Remove the fullnode from arraylist
                FullNodesData.fullNodesRecords.RemoveAt(counter);

                // Persis all data on disk
                this.PersistAllDataOnDisk();
            }
            return found;
        }
        /// <summary>
        /// Save all data on disk
        /// </summary>
        /// <returns>The save operation is successful or not</returns>
        public bool PersistAllDataOnDisk()
        {
            lock (fileLock)
            {
                try
                {
                    // Delete The old one
                    File.Delete(FileStoragePath);

                    // Get all bytes
                    byte[] bytesToWrite = MessagePackSerializer.Serialize(FullNodesData);

                    // Initialize binary writer
                    var fStream = new FileStream(FileStoragePath, FileMode.CreateNew, FileAccess.Write);
                    BinaryWriter bw = new BinaryWriter(fStream);

                    // Writing file binaries on disk...
                    bw.Write(bytesToWrite);
                    
                    // Cleanup!
                    bw.Close();
                    bw.Dispose();
                    fStream.Close();
                    fStream.Dispose();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    return false;
                }
            }
        }
        /// <summary>
        /// Load all data from disk
        /// </summary>
        /// <returns>The operation is successful or not</returns>
        public bool LoadAllDataFromDisk()
        {
            // If there is a file operation on going, the thread must wait
            lock (fileLock)
            {
                try
                {
                    // Initialize file reader
                    var fStream = new FileStream(FileStoragePath, FileMode.Open, FileAccess.Read);

                    // Read all bytes from disk
                    byte[] bytesToRead = File.ReadAllBytes(FileStoragePath);

                    // Convert binary to simple class model
                    FullNodesData = MessagePackSerializer.Deserialize<FullNodesData>(bytesToRead.ToArray());

                    // Cleanup!
                    fStream.Close();
                    fStream.Dispose();
                    
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    return false;
                }
            }
        }
        /// <summary>
        /// The thread to add a new fullnode
        /// </summary>
        public void ListenToAddIpPort()
        {
            // If the main thread is not stopped
            while (!StopThreadRequested)
            {
                try
                {
                    // Listening to IP and port address
                    listenerAdd = new TcpListener(IPAddress.Parse(ObserverData.AddIp), ObserverData.AddPort);
                    listenerAdd.Start();

                    // Wait for call
                    TcpClient client = listenerAdd.AcceptTcpClient();

                    // Get the network stream
                    NetworkStream nwStream = client.GetStream();
                    
                    // The buffer size
                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);

                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    var bytearray = ms.ToArray();
                    
                    // Cleanup!
                    ms.Close();
                    ms.Dispose();
                    client.Close();
                    client.Dispose();
                    listenerAdd.Stop();

                    // Convert binary array to model class
                    FullNodesRecord fNodeRecord = MessagePackSerializer.Deserialize<FullNodesRecord>(bytearray);

                    // Add the record to the list and persist on disk
                    AddNewRecord(fNodeRecord);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }   
        }
        /// <summary>
        /// The thread which listens to remove fullnode from list
        /// </summary>
        public void ListenToRemoveIpPort()
        {
            while (!StopThreadRequested)
            {
                try
                {
                    // Listening to IP address to start removal
                    listenerRemove = new TcpListener(IPAddress.Parse(ObserverData.RemoveIp), ObserverData.RemovePort);
                    listenerRemove.Start();

                    // Initialize the network
                    TcpClient client = listenerRemove.AcceptTcpClient();
                    NetworkStream nwStream = client.GetStream();

                    // Buffer data
                    byte[] data = new byte[1024];

                    MemoryStream ms = new MemoryStream();
                    int numBytesRead = nwStream.Read(data, 0, data.Length);
                    
                    while (numBytesRead > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                        numBytesRead = nwStream.Read(data, 0, data.Length);
                    }

                    // Convert data to class model
                    FullNodesRecord fNodeRecord = MessagePackSerializer.Deserialize<FullNodesRecord>(ms.ToArray());

                    // Removing the node
                    RemoveRecord(fNodeRecord);

                    // Cleanup!
                    client.Close();
                    client.Dispose();
                    listenerRemove.Stop();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }
        /// <summary>
        /// The thread listens to broadcast of network
        /// </summary>
        public void ListenToBroadCastIpPort()
        {
            // Initialize broadcast thread
            listenerBroadCast = new TcpListener(IPAddress.Parse(ObserverData.BroadCastIp), ObserverData.BroadCastPort);
            listenerBroadCast.Start();


            // While stop is not requested from main thread...
            while (!StopThreadRequested)
            {
                try
                {
                    // Wait for request connection
                    TcpClient client = listenerBroadCast.AcceptTcpClient();

                    IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;

                    // If the request ip address is null the thread must be terminated
                    if(remoteIpEndPoint == null) { return ; }
                    
                    // Load the latest information list from disk
                    LoadAllDataFromDisk();

                    // Find the speed line from list for the observer
                    long sendSpeedFromObserverToFullNode = 0;
                    Guid requestedNodeId = new Guid();

                    // Find the node by IP address
                    foreach (var node in FullNodesData.fullNodesRecords)
                    {
                        if (node.SendIP == remoteIpEndPoint.Address.ToString())
                        {
                            requestedNodeId = node.Id;
                            break;
                        }
                    }

                    // Find the bandwidth in the list
                    foreach (var bndwdth in NetworkBandWidth)
                    {
                        if (bndwdth.From == Id && bndwdth.To == requestedNodeId)
                        {
                            sendSpeedFromObserverToFullNode = bndwdth.Speed;
                            break;
                        }
                    }

                    // Send fullnodes data
                    using (NetworkStream nwStream = client.GetStream())
                    {
                        // Serialize in binary format
                        byte[] bytesToSend = MessagePackSerializer.Serialize(FullNodesData);

                        // Simulate the network delay
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

                        // Cleanup!
                        nwStream.Close();
                        nwStream.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ListenToBroadCastIpPort problem" + ex.Message);
                }
            }
        }
        /// <summary>
        /// Cleanup method
        /// </summary>
        public void Dispose()
        {
            StopThreadRequested = true;
            this.listenerAdd.Stop();
            this.listenerRemove.Stop();
            this.listenerBroadCast.Stop();
        }
    }
}
