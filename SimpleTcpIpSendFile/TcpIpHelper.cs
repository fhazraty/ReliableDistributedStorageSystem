﻿using MessagePack;
using System.Net;
using System.Net.Sockets;

namespace SimpleTcpIpSendFile
{
    [MessagePackObject]
    public class MessageModel
    {
        [Key(0)]
        public byte[] data;
        [Key(1)]
        public byte[] signature;
    }
    internal class TcpIpHelper
    {

        public SendFileMode SendFile(string fileToSend,string destinationIpAddress,int destinationPort,byte[] privateKey, byte[] publicKey,int speedBytePerSecond)
        {
            try
            {
                //Read DataFile
                byte[] dataBytes = File.ReadAllBytes(fileToSend);

                CryptographyHelper cHelper = new CryptographyHelper();
                //Create a model contains signature and data
                MessageModel msgModel = new MessageModel()
                {
                    data = dataBytes,
                    signature = cHelper.Sign(cHelper.GetHashSha256ToByte(dataBytes, 0, dataBytes.Length),privateKey,publicKey)
                };
                //Serialize in binary format
                byte[] bytesToSend = MessagePackSerializer.Serialize(msgModel);

                //Prepare TCP/IP Connection
                TcpClient client = new TcpClient(destinationIpAddress, destinationPort);
                NetworkStream nwStream = client.GetStream();

                int iterationCount = bytesToSend.Length / speedBytePerSecond;
                if(bytesToSend.Length % speedBytePerSecond != 0)
                {
                    iterationCount++;
                }

                for (int i = 0; i < iterationCount; i++)
                {
                    DateTime startSendingBuffer = DateTime.Now;

                    if(((i + 1) * speedBytePerSecond) < bytesToSend.Length)
                    {
                        nwStream.Write(bytesToSend, (i * speedBytePerSecond), speedBytePerSecond);
                    }
                    else
                    {
                        nwStream.Write(bytesToSend, (i * speedBytePerSecond), bytesToSend.Length- (i * speedBytePerSecond));
                    }
                    DateTime endSendingBuffer = DateTime.Now;

                    var timeTookToSend = endSendingBuffer.Subtract(startSendingBuffer).TotalMilliseconds;
                    if(timeTookToSend < 1000)
                    {
                        Thread.Sleep(1000 - (int)timeTookToSend);
                    }
                }

                nwStream.Close();
                nwStream.Dispose();

                return
                    new SendFileMode()
                    {
                        IsSuccessful = true
                    };
            }
            catch (Exception ex)
            {
                //Exception occured!
                return new SendFileMode()
                {
                    IsSuccessful = false,
                    Exception = ex
                };
            }
        }

        public SendFileMode ReceiveFile(string fileToReceive, string listeningIpAddress, int listeningPort, byte[] publicKey)
        {
            try
            {
                CryptographyHelper cHelper = new CryptographyHelper();
                //Listening ...
                TcpListener listener = new TcpListener(IPAddress.Parse(listeningIpAddress), listeningPort);
                listener.Start();

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

                MessageModel msgModel = MessagePackSerializer.Deserialize<MessageModel>(ms.ToArray());

                try
                {
                    File.Delete(fileToReceive);
                }
                catch (Exception ex)
                {
                }

                //Storing data on disk ...
                using (var fs = new FileStream(fileToReceive, FileMode.CreateNew, FileAccess.Write))
                {
                    fs.Write(msgModel.data, 0, msgModel.data.Length);
                }

                //Closing connection ...
                client.Close();
                client.Dispose();
                listener.Stop();
                return
                    new SendFileMode()
                    {
                        IsSuccessful = true,
                        Hash = cHelper.Verify(cHelper.GetHashSha256ToByte(msgModel.data, 0, msgModel.data.Length), msgModel.signature,publicKey).ToString()
                    };
            }
            catch (Exception ex)
            {
                //Exception occured!
                return new SendFileMode()
                {
                    IsSuccessful = false,
                    Exception = ex
                };
            }
        }
    }

    public class SendFileMode
    {
        public bool IsSuccessful { get; set; }
        public Exception? Exception { get; set; }
        public string? Hash { get; set; }
    }
}
