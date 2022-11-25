using FullFunctionProject;
using Model;
using Observer;

//Main path to store miners data
var mainPath = @"c:\Miners\";

//Delete old files
try { Directory.Delete(mainPath, true); } catch { }

// General Info initialization
var observerData = new ObserverData("127.0.0.1", 7000, "127.0.0.1", 7001, "127.0.0.1", 7002);

// Number of full nodes and listening ports
var numberOfFullNodes = 10;
var sendPortStartRange = 5000;
var receivePortStartRange = 6000;

// Network Bandwidth initialization
// 10 nodes and one observer
// 0 means no connection
// 10*1024*1024

var networkBandWidth = new List<SpeedLine>();
List<Guid> nodeIdList = new List<Guid>();
for (int i = 0; i < numberOfFullNodes; i++)
{
    nodeIdList.Add(Guid.NewGuid());
}

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

var fullNodes = new List<FullNode.Node>();
// Initializing full nodes ranges
for (int i = 0; i < numberOfFullNodes; i++)
{
    fullNodes.Add
    (
        new FullNode.Node(
            nodeIdList[i],
            observerData, 
            "127.0.0.1", 
            sendPortStartRange++, 
            "127.0.0.1", 
            receivePortStartRange++, 
            networkBandWidth, 
            i, 
            mainPath, // c:\Miners\
            50,
            10000,
            50)
    );
}



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

    Starter starter = new Starter();
    Thread T = new Thread(new ParameterizedThreadStart(starter.StartNodeThread));
    T.Start(startModel);
}

Console.ReadKey();
