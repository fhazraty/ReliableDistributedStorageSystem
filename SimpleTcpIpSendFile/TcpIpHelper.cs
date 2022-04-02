using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleTcpIpSendFile
{
    internal class TcpIpHelper
    {
        public SendFileMode SendFile(string fileToSend,string destinationIpAddress,int destinationPort)
        {
            try
            {
                CryptographyHelper cHelper = new CryptographyHelper();
                TcpClient client = new TcpClient(destinationIpAddress, destinationPort);
                NetworkStream nwStream = client.GetStream();

                //ReadFile
                byte[] bytesToSend = File.ReadAllBytes(fileToSend);

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
                            Hash = cHelper.GetHashSha256(bytesToSend, 0, bytesToSend.Length)
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

        public SendFileMode ReceiveFile(string fileToReceive, string listeningIpAddress, int listeningPort)
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

                //Storing data on disk ...
                using (var fs = new FileStream(fileToReceive, FileMode.CreateNew, FileAccess.Write))
                {
                    fs.Write(buffer, 0, bytesRead);
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
                        Hash = cHelper.GetHashSha256(buffer, 0, bytesRead)
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
