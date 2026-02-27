using System.Linq.Expressions;
using backend_net.Data.Context;
using backend_net.Data.Interfaces;
using backend_net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend_net.Data.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async System.Threading.Tasks.Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // EF Core optimizes multiple Where() calls, but combining is more explicit
        // Create combined predicate: !IsDeleted && predicate
        var parameter = Expression.Parameter(typeof(T), "e");
        var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var notDeleted = Expression.Not(isDeletedProperty);
        var combinedPredicate = Expression.AndAlso(notDeleted, 
            new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body));
        var lambda = Expression.Lambda<Func<T, bool>>(combinedPredicate, parameter);
        
        return await _dbSet.Where(lambda).ToListAsync();
    }

    public virtual async System.Threading.Tasks.Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        // Combine soft delete filter with predicate for better performance
        var parameter = Expression.Parameter(typeof(T), "e");
        var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var notDeleted = Expression.Not(isDeletedProperty);
        var combinedPredicate = Expression.AndAlso(notDeleted, 
            new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body));
        var lambda = Expression.Lambda<Func<T, bool>>(combinedPredicate, parameter);
        
        return await _dbSet.Where(lambda).FirstOrDefaultAsync();
    }
    
    // Helper class to replace expression parameters
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    public virtual async System.Threading.Tasks.Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async System.Threading.Tasks.Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }
        await _dbSet.AddRangeAsync(entityList);
        return entityList;
    }

    public virtual System.Threading.Tasks.Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public virtual System.Threading.Tasks.Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public virtual async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    public virtual async System.Threading.Tasks.Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.IsDeleted).AnyAsync(predicate);
    }

    public virtual async System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.Where(e => !e.IsDeleted).CountAsync();
        }
        return await _dbSet.Where(e => !e.IsDeleted).CountAsync(predicate);
    }

    public virtual async System.Threading.Tasks.Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

