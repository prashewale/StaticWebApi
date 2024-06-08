using Static.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.Repository
{
    // singleton
    internal class MockRepository : IRepository<ApiKeyModel>
    {
        List<ApiKeyModel> ListOfApiKeys { get; set; } = new List<ApiKeyModel>();

        public Task CreateAllAsync(IEnumerable<ApiKeyModel> entities)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiKeyModel> CreateAsync(ApiKeyModel entity)
        {
            int newId = ListOfApiKeys.Count + 1;

            entity.Id = newId;

            ListOfApiKeys.Add(entity);

            return entity;
        }

        public Task<ApiKeyModel> DeleteAsync(ApiKeyModel entity)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKeyModel> DeleteByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<ApiKeyModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IQueryable<ApiKeyModel>> GetAllAsync(Expression<Func<ApiKeyModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKeyModel?> GetByAsync(Expression<Func<ApiKeyModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKeyModel?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAllAsync(IEnumerable<ApiKeyModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKeyModel> UpdateAsync(ApiKeyModel entity)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAllAsync(IEnumerable<ApiKeyModel> entities)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKeyModel> UpsertAsync(ApiKeyModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
