using Observer;

var cManager = new ConnectionManager();
var res1 = cManager.AddNewRecord(new FullNodesRecord()
{
    IPAddress = "127.0.0.1",
    PortSend = 5000,
    PortReceive = 5001
});

Console.WriteLine(res1);

var res2 = cManager.AddNewRecord(new FullNodesRecord()
{
    IPAddress = "127.0.0.2",
    PortSend = 5000,
    PortReceive = 5001
});

Console.WriteLine(res2);

var res3 = cManager.AddNewRecord(new FullNodesRecord()
{
    IPAddress = "127.0.0.3",
    PortSend = 5000,
    PortReceive = 5001
});

Console.WriteLine(res3);

var res4 = cManager.AddNewRecord(new FullNodesRecord()
{
    IPAddress = "127.0.0.4",
    PortSend = 5000,
    PortReceive = 5001
});

Console.WriteLine(res4);

var res5 = cManager.RemoveRecord("127.0.0.3");

Console.WriteLine(res5);


Console.WriteLine(cManager.FullNodesData.fullNodesRecords.Count());

Console.ReadKey();