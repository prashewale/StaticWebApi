using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Static.Services.Models;
using Static.Services.Repository.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.Repository
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : FullAuditModel
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TEntity> UpsertAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            if (entity.Id == 0)
                return await CreateAsync(entity);
            else
                return await UpdateAsync(entity);
        }
        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            if(entity.Id != 0) throw new ArgumentException($"{nameof(entity.Id)} is should be zero.");

            await _dbContext.Set<TEntity>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<TEntity> DeleteAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            _dbContext.Set<TEntity>().Remove(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<TEntity> DeleteByIdAsync(int id)
        {
            if(id <= 0) throw new ArgumentException($"{nameof(id)} should be greater than zero.");

            var availableEntity = await GetByIdAsync(id);
            ArgumentNullException.ThrowIfNull(availableEntity, nameof(availableEntity));

            return await DeleteAsync(availableEntity);
        }

        public async Task<IQueryable<TEntity>> GetAllAsync()
        {
            return _dbContext.Set<TEntity>().AsNoTracking();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
           return  await _dbContext.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Id == 0) throw new ArgumentException($"{nameof(entity.Id)} is should not be zero.");

            _dbContext.Set<TEntity>().Update(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }



        public async Task UpdateAllAsync(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));

            if (entities.Any(x => x.Id == 0))
                throw new ArgumentException($"{nameof(FullAuditModel.Id)} is should not be zero.");

            _dbContext.Set<TEntity>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateAllAsync(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));

            if(entities.Any(x => x.Id != 0))
                throw new ArgumentException($"{nameof(FullAuditModel.Id)} is should be zero.");

            await _dbContext.Set<TEntity>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpsertAllAsync(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));

            var entriesToBeAdded = entities.Where(x => x.Id == 0);
            var entriesToBeUpdated = entities.Where(x => x.Id > 0);

            if (entriesToBeAdded.Any())
            {
                await CreateAllAsync(entriesToBeAdded);
            }

            if(entriesToBeUpdated.Any())
            {
                await UpdateAllAsync(entriesToBeUpdated);
            }
        }

        public async Task<TEntity?> GetByAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

            return await _dbContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public async Task<IQueryable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
            return _dbContext.Set<TEntity>().AsNoTracking().Where(predicate);
        }

        //~GenericRepository()
        //{
        //    _dbContext.Dispose();
        //}
    }
}
