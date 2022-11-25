using Model;

namespace Observer
{
    public class ObserverNode
    {
        public ConnectionManager ConnectionManager { get; set; }
        public ObserverNode(ObserverData observerData, List<SpeedLine> networkBandWidth, string mainPath)
        {
            ConnectionManager = new ConnectionManager(observerData, networkBandWidth, mainPath);

            var addFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToAddIpPort));
            addFullNodeThread.Start();

            var removeFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToRemoveIpPort));
            removeFullNodeThread.Start();

            var observeFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToBroadCastIpPort));
            observeFullNodeThread.Start();
        }
    }
}
