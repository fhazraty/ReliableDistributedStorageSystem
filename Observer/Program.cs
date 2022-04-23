using Model;
using Observer;

//static ip port
string observerBroadCastIp = "127.0.0.1";
int observerBroadCastPort = 8000;

string observerAddIp = "127.0.0.1";
int observerAddPort = 8001;

string observerRemoveIp = "127.0.0.1";
int observerRemovePort = 8002;

var cManager = new ConnectionManager(new ObserverData()
{
    BroadCastIp = observerBroadCastIp,
    BroadCastPort = observerBroadCastPort,
    AddIp = observerAddIp,
    AddPort = observerAddPort,
    RemovePort = observerRemovePort,
    RemoveIp = observerRemoveIp,
});

Thread addFullNodeThread = new Thread(new ThreadStart(cManager.ListenToAddIpPort));
addFullNodeThread.Start();

Thread removeFullNodeThread = new Thread(new ThreadStart(cManager.ListenToRemoveIpPort));
removeFullNodeThread.Start();

Console.ReadKey();