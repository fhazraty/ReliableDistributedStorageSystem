using FullFunctionProject;
using Model;
using Observer;

//Main path to store miners data
var mainPath = @"c:\Miners\";

//Cleansing
//Delete old files and folders
try 
{ 
    Directory.Delete(mainPath, true); 
} 
catch(DirectoryNotFoundException nfex)
{
    Console.WriteLine("Directory not found but the program can continue ...");
}
catch(Exception ex)
{
    Console.WriteLine("Error in removing old files and folders ...");
    Console.WriteLine(ex.Message);
    
    //Waiting...
    Console.ReadKey();
    
    //Prevent running program...
    return;
}

// General Info initialization
var observerData = new ObserverData("127.0.0.1", 7000, "127.0.0.1", 7001, "127.0.0.1", 7002);

// Number of full nodes and listening ports
var numberOfFullNodes = 3;
var sendPortStartRange = 5000;
var receivePortStartRange = 6000;

// Network Bandwidth initialization
// 10 nodes and one observer
// 0 means no connection
// 10*1024*1024

var networkBandWidth = new List<SpeedLine>();

//Keeping id of full nodes for future tracking ...
List<Guid> nodeIdList = new List<Guid>();

//Adding new guid to each full node
for (int i = 0; i < numberOfFullNodes; i++)
{
    nodeIdList.Add(Guid.NewGuid());
}

//Initializing bandwidth array
for (int i = 0; i < nodeIdList.Count; i++)
{
    for (int j = 0; j < nodeIdList.Count; j++)
    {
        if (i == j)
        {
            networkBandWidth.Add(new SpeedLine() { From = nodeIdList[i], To = nodeIdList[j],Speed = 0 });
        }
        else
        {
            networkBandWidth.Add(new SpeedLine() { From = nodeIdList[i], To = nodeIdList[j], Speed = 10*1024*1024 });
        }
    }
}

// Starting observer
var observerNode = new ObserverNode(observerData, networkBandWidth, mainPath);

//Declaring fullnodes list
var fullNodes = new List<FullNode.Node>();

// Initializing fullnodes  ip address and ports
for (int i = 0; i < numberOfFullNodes; i++)
{
    fullNodes.Add
    (
        new FullNode.Node(
            //Id of fullnode
            nodeIdList[i],
            //Observer data include ip address, port and etc...
            observerData, 
            //The ip address of full node to send data
            "127.0.0.1",
            //The port address of full node to send data
            sendPortStartRange++,
            //The ip address of full node to receive data
            "127.0.0.1",
            //The port address of full node to receive data
            receivePortStartRange++, 
            //The network bandwidth array to simulate the network delay
            networkBandWidth,
            //The storage path to save all data
            mainPath, 
            //Sleep between retry connecting to observer node
            50,
            //The number of retries to connect to observer
            10000,
            //The randomize parameter of sleeping by multiply a random number between 0 and this number to sleeptime parameter
            50,
            //Sleep between retry sending files
            50,
            //The number of retries to send files
            100,
            //The randomize parameter of sleeping by multiply a random number between 0 and this number to sleeptime parameter for sending files
            50)
    );
}

//Sending a transaction to each node
foreach (var fullNode in fullNodes)
{
    StarterModel startModel = new StarterModel()
    {
        FileName = "I Walk Alone_v720P.mp4",
        FileContent = File.ReadAllBytes(@"C:\Users\farhad\Downloads\Video\I Walk Alone_v720P.mp4"),
        SleepRetryObserver = 50,
        NumberOfRetryObserver = 50,
        SleepRetrySendFile = 1000,
        NumberOfRetrySendFile = 3,
        RandomizeRangeSleep = 1000,
        Node = fullNode
    };

    //Using a bootstrapper to start nodes as an independant thread
    Starter starter = new Starter();
    Thread T = new Thread(new ParameterizedThreadStart(starter.StartNodeThread));
    T.Start(startModel);
}

//Waiting for program to continue...
Console.ReadKey();
