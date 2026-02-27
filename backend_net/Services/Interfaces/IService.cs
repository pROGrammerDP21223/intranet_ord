using System.Linq.Expressions;
using backend_net.Domain.Entities;

namespace backend_net.Services.Interfaces;

public interface IService<T> where T : BaseEntity
{
    System.Threading.Tasks.Task<T?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<T> CreateAsync(T entity);
    System.Threading.Tasks.Task<T> UpdateAsync(T entity);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

