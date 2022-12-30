using FullNodeDataLogger.EF.Model;
using FullNodeDataLogger.Model;
using FullNodeDataLogger.Repository;

namespace FullNodeDataLogger.Management
{
    public class LogManagement : ILogManagement
    {
        public ILogRepository LogRepository { get; set; }

        public LogManagement(ILogRepository logRepository)
        {
            this.LogRepository = logRepository;
        }

        public IBaseResult MakeLog(Log log)
        {
            log.LogTime = DateTime.Now;

            return LogRepository.Create(log);
        }
    }
}
