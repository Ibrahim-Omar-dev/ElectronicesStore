using ElectronicsStore.Data;
using ElectronicsStore.RepositoryAndUnitOfWork;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
        else
        {
            throw new KeyNotFoundException($"Entity with id {id} not found.");
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task SaveChangeAsync()
    {
        _context.SaveChanges();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
    }
    public async Task<(List<T> Items, int TotalPages)> GetPage(int pageSize, int pageIndex)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 5;

        var totalCount = await _dbSet.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await _dbSet
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalPages);
    }
}