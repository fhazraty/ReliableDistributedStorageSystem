using Model;

// General Info initialization
ObserverData observerData = new ObserverData("127.0.0.1",7000,"127.0.0.1",7001,"127.0.0.1",7002);

// Number of full nodes and listening ports
int numberOfFullNodes = 10;
int sendPortStartRange = 5000;
int receivePortStartRange = 6000;

// Network Bandwidth initialization
// 10 nodes and one observer
// 0 means no connection

List<List<long>> networkBandWidth = new List<List<long>>();
for (int i = 0; i < 11; i++)
{
    List<long> row = new List<long>();
    for (int j = 0; j < 11; j++)
    {
        if (i == j)
        {
            row.Add(0);
        }
        else
        {
            row.Add(1024*1024);
        }
    }

    networkBandWidth.Add(row);
}
    
// Starting observer
var ocManager = new Observer.ConnectionManager(observerData,networkBandWidth);

Thread addFullNodeThread = new Thread(new ThreadStart(ocManager.ListenToAddIpPort));
addFullNodeThread.Start();

Thread removeFullNodeThread = new Thread(new ThreadStart(ocManager.ListenToRemoveIpPort));
removeFullNodeThread.Start();

Thread observeFullNodeThread = new Thread(new ThreadStart(ocManager.ListenToBroadCastIpPort));
observeFullNodeThread.Start();

List<FullNode.Node> fullNodes = new List<FullNode.Node>();

// Initializing full nodes ranges
for (int i = 0; i < numberOfFullNodes; i++)
{
    fullNodes.Add
    (
        new FullNode.Node(observerData, "127.0.0.1",sendPortStartRange++,"127.0.0.1",receivePortStartRange++,networkBandWidth,i)
    );
}

var res = fullNodes[0].SaveFile("I Walk Alone_v720P.mp4", File.ReadAllBytes(@"C:\Users\farhad\Downloads\Video\I Walk Alone_v720P.mp4"));


