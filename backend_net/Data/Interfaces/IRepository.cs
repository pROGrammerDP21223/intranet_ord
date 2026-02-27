using System.Linq.Expressions;
using backend_net.Domain.Entities;

namespace backend_net.Data.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    System.Threading.Tasks.Task<T?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<T> AddAsync(T entity);
    System.Threading.Tasks.Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    System.Threading.Tasks.Task UpdateAsync(T entity);
    System.Threading.Tasks.Task DeleteAsync(T entity);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    System.Threading.Tasks.Task SaveChangesAsync();
}

