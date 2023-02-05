using Model;
using System.Text;
using Utilities;

var cHelper = new CryptographyHelper();
byte[] publicKey;
byte[] privateKey;
cHelper.GenerateKeyRSA(out privateKey, out publicKey);
string originalMessage = "Hello!";

var signedData = cHelper.SignRSA(Encoding.UTF8.GetBytes(originalMessage),privateKey);
var resultOfChecking = cHelper.VerifyRSA(Encoding.UTF8.GetBytes(originalMessage), signedData, publicKey);
Console.WriteLine(resultOfChecking);

Console.ReadKey();

/*
Console.WriteLine("Simple TCP IP Send and receive File:");
Console.WriteLine("Press 0 to Generate keys :");
Console.WriteLine("Press 1 to send or 2 to receive :");

var command = Console.ReadLine();

string fileToSend = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\humans.s02e02.720p.x265.mkv";
string fileToReceive = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\humans.s02e02.720p.x265-2.mkv";


string pubKey = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\pub.txt";
string priKey = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Sim\SimpleTcpIpSendFile\bin\pri.txt";

var helper = new TcpIpHelper();

if(command == "0")
{
    var cHelper = new CryptographyHelper();
    byte[] publicKey;
    byte[] privateKey;
    cHelper.GenerateKey(out privateKey,out publicKey);
    //Storing private key on disk ...
    using (var fs = new FileStream(priKey, FileMode.CreateNew, FileAccess.Write))
    {
        fs.Write(privateKey, 0, privateKey.Length);
    }
    //Storing public key on disk ...
    using (var fs = new FileStream(pubKey, FileMode.CreateNew, FileAccess.Write))
    {
        fs.Write(publicKey, 0, publicKey.Length);
    }
}
else if (command == "1")
{
    byte[] privateKeyBytes = File.ReadAllBytes(priKey);
    byte[] publicKeyBytes = File.ReadAllBytes(pubKey);

    var sendResult = helper.SendFile(fileToSend,"127.0.0.1",5000,privateKeyBytes,publicKeyBytes,5010240);
    Console.WriteLine("Send result :{0}", sendResult.Successful);
    if (sendResult.Successful)
    {
        Console.WriteLine("Hash :{0}", ((SendFileMode)sendResult).ResultContainer);
    }
}
else
{
    byte[] publicKeyBytes = File.ReadAllBytes(pubKey);

    var receiveResult = helper.ReceiveFile(fileToReceive, "127.0.0.1", 5000,publicKeyBytes);
    Console.WriteLine("Receive result :{0}", receiveResult.Successful);
    if(receiveResult.Successful)
    {
        Console.WriteLine("Hash :{0}",((SendFileMode) receiveResult).ResultContainer);
    }
    
}
Console.ReadKey();
*/
