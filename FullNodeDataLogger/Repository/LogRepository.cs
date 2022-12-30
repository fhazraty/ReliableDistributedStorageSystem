using FullNodeDataLogger.EF;
using FullNodeDataLogger.EF.Model;
using FullNodeDataLogger.Model;

namespace FullNodeDataLogger.Repository
{
    public class LogRepository : ILogRepository
    {
        private FullNodeDataLoggerEntities context;
        public LogRepository(FullNodeDataLoggerEntities context)
        {
            this.context = context;
        }
        public Log GetByParams(Log entity)
        {
            throw new NotImplementedException();
        }

        public Log GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public IBaseResult Create(Log entity)
        {
            try
            {
                context.Logs.Add(entity);
                context.SaveChanges();
                return new CreateEntityResult()
                {
                    Successful = true,
                    ResultContainer = entity.Id
                };
            }
            catch (Exception ex)
            {
                return new ErrorResult()
                {
                    Successful = false,
                    ResultContainer = ex
                };
            }
        }

        public IBaseResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IBaseResult Update(Log entity)
        {
            throw new NotImplementedException();
        }

        public List<Log> ListAll(int skip, int take)
        {
            throw new NotImplementedException();
        }

        public List<Log> ListAll(int skip, int take, string lang)
        {
            throw new NotImplementedException();
        }

        public Log GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IBaseResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
