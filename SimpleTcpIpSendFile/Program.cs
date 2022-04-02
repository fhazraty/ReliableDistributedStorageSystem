using SimpleTcpIpSendFile;

Console.WriteLine("Simple TCP IP Send and receive File:");
Console.WriteLine("Press 1 to send or 2 to receive :");

var command = Console.ReadLine();

string fileToSend = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Simulator\SimpleTcpIpSendFile\bin\TestToSend.txt";
string fileToReceive = @"C:\Users\Farhad\Desktop\PHD\Paper\A Reliable Distributed Storage System using Blockchain Networks\Simulator\SimpleTcpIpSendFile\bin\TestToReceive.txt";

var helper = new TcpIpHelper();

if (command == "1")
{
    var sendResult = helper.SendFile(fileToSend,"127.0.0.1",5000);
    Console.WriteLine("Send result :{0}", sendResult.IsSuccessful);
    Console.WriteLine("Hash :{0}", sendResult.Hash);
}
else
{
    var receiveResult = helper.ReceiveFile(fileToReceive, "127.0.0.1", 5000);
    Console.WriteLine("Receive result :{0}", receiveResult.IsSuccessful);
    Console.WriteLine("Hash :{0}", receiveResult.Hash);
}

Console.ReadKey();