using SimpleTcpIpSendFile;

Console.WriteLine("Simple TCP IP Send and receive File:");
Console.WriteLine("Press 0 to Generate keys :");
Console.WriteLine("Press 1 to send or 2 to receive :");

var command = Console.ReadLine();

string fileToSend = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\TestToSend.txt";
string fileToReceive = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\TestToReceive.txt";


string pubKey = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\pub.txt";
string priKey = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\pri.txt";

var helper = new TcpIpHelper();

if(command == "0")
{
    var cHelper = new CryptographyHelper();
    byte[] publicKey;
    byte[] privateKey;
    cHelper.GenerateKey(out privateKey,out publicKey);
    //Storing data on disk ...
    using (var fs = new FileStream(priKey, FileMode.CreateNew, FileAccess.Write))
    {
        fs.Write(privateKey, 0, privateKey.Length);
    }
    //Storing data on disk ...
    using (var fs = new FileStream(pubKey, FileMode.CreateNew, FileAccess.Write))
    {
        fs.Write(publicKey, 0, publicKey.Length);
    }
}
else if (command == "1")
{
    byte[] privateKeyBytes = File.ReadAllBytes(priKey);
    byte[] publicKeyBytes = File.ReadAllBytes(pubKey);

    var sendResult = helper.SendFile(fileToSend,"127.0.0.1",5000,privateKeyBytes,publicKeyBytes);
    Console.WriteLine("Send result :{0}", sendResult.IsSuccessful);
    Console.WriteLine("Hash :{0}", sendResult.Hash);
}
else
{
    byte[] publicKeyBytes = File.ReadAllBytes(pubKey);

    var receiveResult = helper.ReceiveFile(fileToReceive, "127.0.0.1", 5000,publicKeyBytes);
    Console.WriteLine("Receive result :{0}", receiveResult.IsSuccessful);
    Console.WriteLine("Hash :{0}", receiveResult.Hash);
}

Console.ReadKey();