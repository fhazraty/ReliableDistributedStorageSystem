using MessagePack;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

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

        public SendFileMode SendFile(string fileToSend,string destinationIpAddress,int destinationPort,byte[] privateKey, byte[] publicKey)
        {
            try
            {
                CryptographyHelper cHelper = new CryptographyHelper();

                //ReadFile
                byte[] dataBytes = File.ReadAllBytes(fileToSend);

                MessageModel msgModel = new MessageModel()
                {
                    data = dataBytes,
                    signature = cHelper.Sign(cHelper.GetHashSha256ToByte(dataBytes, 0, dataBytes.Length),privateKey,publicKey)
                };

                byte[] bytesToSend = MessagePackSerializer.Serialize(msgModel);

                TcpClient client = new TcpClient(destinationIpAddress, destinationPort);
                NetworkStream nwStream = client.GetStream();
                //Sending File ...
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                //Read Result ...
                byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                client.Close();
                if (Encoding.ASCII.GetString(bytesToRead, 0, bytesRead) == "done")
                {
                    return
                        new SendFileMode()
                        {
                            IsSuccessful = true,
                            Hash = ""
                        };

                }
                //done is not received!
                return
                    new SendFileMode()
                    {
                        IsSuccessful = false
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
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                MessageModel msgModel = MessagePackSerializer.Deserialize<MessageModel>(buffer);

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

                //Sending Ack ...
                var acc = UTF8Encoding.UTF8.GetBytes("done");
                nwStream.Write(acc, 0, acc.Length);

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
