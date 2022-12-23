using FullNode;

namespace FullFunctionProject
{
    public class Starter
    {
        public void StartNodeThread(object obj)
        {
            StarterModel starter = (StarterModel)obj;
            starter.Node.SaveFile
                (starter.FileName,
                starter.FileContent,
                starter.SleepRetryObserver,
                starter.NumberOfRetryObserver,
                starter.SleepRetrySendFile,
                starter.NumberOfRetrySendFile,
                starter.RandomizeRangeSleep
                );
        }
        
    }
    /// <summary>
    /// The model which is required for starting send transaction thread
    /// </summary>
    public class StarterModel 
    {
        // The name of file for sending as a transaction
        public string FileName { get; set; }
        
        // The binary array of file
        public byte[] FileContent { get; set; }
        
        // The sleep after retry to connect to observer
        public int SleepRetryObserver { get; set; }
        
        // The number of retry to connect to observer
        public int NumberOfRetryObserver { get; set; }
        
        // The delay for retry sending file
        public int SleepRetrySendFile { get; set; }
        
        // The number of retry for sending file in network
        public int NumberOfRetrySendFile { get; set; }
        
        // The random number for the sleep range
        public int RandomizeRangeSleep { get; set; }
        
        // The node which starts
        public Node Node { get; set; }
    }

}
