using FullNodeDataLogger.EF.Model;
using FullNodeDataLogger.Model;

namespace FullNodeDataLogger.Management
{
    public interface ILogManagement
    {
        IBaseResult MakeLog(Log log);
    }
}
