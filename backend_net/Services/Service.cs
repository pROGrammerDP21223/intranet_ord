using System.Linq.Expressions;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using backend_net.Services.Interfaces;

namespace backend_net.Services;

public class Service<T> : IService<T> where T : BaseEntity
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IRepository<T> _repository;

    public Service(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = _unitOfWork.Repository<T>();
    }

    public virtual async System.Threading.Tasks.Task<T?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public virtual async System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.FindAsync(predicate);
    }

    public virtual async System.Threading.Tasks.Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.FirstOrDefaultAsync(predicate);
    }

    public virtual async System.Threading.Tasks.Task<T> CreateAsync(T entity)
    {
        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity;
    }

    public virtual async System.Threading.Tasks.Task<T> UpdateAsync(T entity)
    {
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity;
    }

    public virtual async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public virtual async System.Threading.Tasks.Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.ExistsAsync(predicate);
    }

    public virtual async System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return await _repository.CountAsync(predicate);
    }
}

