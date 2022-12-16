namespace Model
{
    public class ObserverData
    {
        /// <summary>
        /// Data required for initializing an observer
        /// </summary>
        /// <param name="broadCastIp">Broadcasting IP address</param>
        /// <param name="broadCastPort">Broadcasting port</param>
        /// <param name="addIp">Add new node IP address</param>
        /// <param name="addPort">Add new node port</param>
        /// <param name="removeIp">Remove node IP address</param>
        /// <param name="removePort">Remove node port</param>
        public ObserverData(string broadCastIp,int broadCastPort,string addIp,int addPort,string removeIp,int removePort)
        {
            this.BroadCastIp = broadCastIp;
            this.BroadCastPort = broadCastPort;
            this.AddIp = addIp;
            this.AddPort = addPort;
            this.RemoveIp = removeIp;
            this.RemovePort = removePort;
        }
        public string BroadCastIp { get; set; }
        public int BroadCastPort { get; set; }
        public string AddIp { get; set; }
        public int AddPort { get; set; }
        public string RemoveIp { get; set; }
        public int RemovePort { get; set; }
    }
}
