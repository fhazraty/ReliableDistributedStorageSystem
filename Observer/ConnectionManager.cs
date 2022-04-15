using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    public class ConnectionManager
    {
        public string FileStoragePath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\P2PStorage.txt";
            }
        }

        public FullNodesData FullNodesData { get; set; }

        public ConnectionManager()
        {
            FullNodesData = new FullNodesData();

            LoadAllDataFromDisk();
        }

        public bool AddNewRecord(FullNodesRecord record)
        {
            int counter = -1;
            bool found = false;
            foreach (var recordSearch in FullNodesData.fullNodesRecords)
            {
                counter++;
                if (recordSearch.IPAddress.ToString() == record.IPAddress.ToString())
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                RemoveRecord(record.IPAddress);
            }

            FullNodesData.fullNodesRecords.Add(record);
            
            this.PersistAllDataOnDisk();
            
            return true;
        }

        public bool RemoveRecord(string ip)
        {
            int counter = -1;
            bool found = false;
            foreach (var record in FullNodesData.fullNodesRecords)
            {
                counter++;
                if (record.IPAddress.ToString() == ip.ToString())
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

        public bool LoadAllDataFromDisk()
        {
            try
            {
                var fStream = new FileStream(FileStoragePath, FileMode.Open, FileAccess.Read);
                
                byte[] bytesToRead = File.ReadAllBytes(FileStoragePath);

                FullNodesData = MessagePackSerializer.Deserialize<FullNodesData>(bytesToRead.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
