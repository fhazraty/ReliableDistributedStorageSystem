using FullNodeDataLogger.Model;

namespace FullNodeDataLogger.Repository
{
    public interface IRepository<T, TKey>
    {
        T GetByParams(T entity);
        T GetById(TKey id);
        IBaseResult Create(T entity);
        IBaseResult Delete(TKey id);
        IBaseResult Update(T entity);
        List<T> ListAll(int skip, int take);
        List<T> ListAll(int skip, int take, string query);
    }
}
