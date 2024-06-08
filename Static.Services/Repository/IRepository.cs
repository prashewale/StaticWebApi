using Static.Services.Models;
using System;
using System.Linq.Expressions;

namespace Static.Services.Repository;

public interface IRepository<T> where T : IIdentity
{
    Task<T> UpsertAsync(T entity);
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T> DeleteAsync(T entity);
    Task<T> DeleteByIdAsync(int id);
    Task<IQueryable<T>> GetAllAsync();

    Task CreateAllAsync(IEnumerable<T> entities);
    Task UpdateAllAsync(IEnumerable<T> entities);
    Task UpsertAllAsync(IEnumerable<T> entities);
    Task<T?> GetByAsync(Expression<Func<T, bool>> predicate);
    Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

}
