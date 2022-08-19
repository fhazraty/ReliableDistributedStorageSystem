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
var networkBandWidth = new List<List<long>>();
for (int i = 0; i < numberOfFullNodes + 1; i++)
{
    var row = new List<long>();
    for (int j = 0; j < numberOfFullNodes + 1; j++)
    {
        if (i == j) 
            row.Add(0); 
        else 
            row.Add(10*1024*1024);
    }
    networkBandWidth.Add(row);
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
            observerData, 
            "127.0.0.1", 
            sendPortStartRange++, 
            "127.0.0.1", 
            receivePortStartRange++, 
            networkBandWidth, 
            i, 
            mainPath,
            100,
            100,
            50)
    );
}

Parallel.ForEach(fullNodes, fullNode =>
{
    //Submit file to network...
    var res = fullNode.SaveFile("I Walk Alone_v720P.mp4", File.ReadAllBytes(@"C:\Users\farhad\Downloads\Video\I Walk Alone_v720P.mp4"),100,100,100,100,50);
});

