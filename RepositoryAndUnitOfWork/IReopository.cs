namespace ElectronicsStore.RepositoryAndUnitOfWork
{
    public interface IRepository<T> where T : class

    {
        public Task AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(int id);
        public Task<T?> GetByIdAsync(int id);
        public Task<IEnumerable<T>> GetAllAsync();
        public Task SaveChangeAsync();
        public Task<(List<T> Items, int TotalPages)> GetPage(int pageSize, int pageIndex);
    }
}
