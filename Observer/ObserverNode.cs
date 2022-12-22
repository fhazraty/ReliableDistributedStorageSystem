using Model;

namespace Observer
{
    /// <summary>
    /// In this netwrok there must be at least one observer node to hold fullnodes ip addresses and certificates
    /// </summary>
    public class ObserverNode
    {
        public Guid Id { get; set; }
        public ConnectionManager ConnectionManager { get; set; }
        /// <summary>
        /// Builds a new observer node
        /// </summary>
        /// <param name="observerData">The data about observer ip address and ports</param>
        /// <param name="networkBandWidth">The bandwidth of network</param>
        /// <param name="mainPath">The path for database storage</param>
        public ObserverNode(ObserverData observerData, List<SpeedLine> networkBandWidth, string mainPath, Guid id)
        {
            Id = id;

            // Initialize the connection manager of observer
            ConnectionManager = new ConnectionManager(observerData, networkBandWidth, mainPath, id);

            // The thread for add new fullnodes
            var addFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToAddIpPort));
            addFullNodeThread.Start();

            // The thread for remove fullnodes
            var removeFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToRemoveIpPort));
            removeFullNodeThread.Start();

            // The thread for broadcasting fullnodes data
            var observeFullNodeThread = new Thread(new ThreadStart(ConnectionManager.ListenToBroadCastIpPort));
            observeFullNodeThread.Start();
        }
    }
}
