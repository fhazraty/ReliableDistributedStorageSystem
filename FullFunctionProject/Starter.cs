using FullNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class StarterModel 
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public int SleepRetryObserver { get; set; }
        public int NumberOfRetryObserver { get; set; }
        public int SleepRetrySendFile { get; set; }
        public int NumberOfRetrySendFile { get; set; }
        public int RandomizeRangeSleep { get; set; }
        public Node Node { get; set; }
    }

}
