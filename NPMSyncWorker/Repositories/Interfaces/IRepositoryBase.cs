namespace NPMSyncWorker.Repositories.Interfaces
{
    internal interface IRepositoryBase<T> where T : class
    {
        int Add(T entity);
        int Update(T entity);
    }
}
