using Acme.BookStore.Authors;
using Acme.BookStore.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Acme.BookStore.Departments
{
    public class DepartmentRepository : EfCoreRepository<BookStoreDbContext, Department, Guid>,
        IDepartmentRepository
    {
        private readonly IDbContextProvider<BookStoreDbContext> dbContextProvider;

        public DepartmentRepository(
        IDbContextProvider<BookStoreDbContext> dbContextProvider)
        : base(dbContextProvider)
        {
            this.dbContextProvider = dbContextProvider;
        }

        private static readonly Func<BookStoreDbContext, Guid, Task<Department>> GetByIdQuery = 
            EF.CompileAsyncQuery((BookStoreDbContext db, Guid id) =>
            db.Departments.AsNoTracking().FirstOrDefault(c => c.Id == id));

        public async Task<Department> FindIdAsync(Guid id)
        {
            var dbContext = dbContextProvider.GetDbContext();
            return await dbContext.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }
        
        public async Task<Department> FindIdUsingCompiledQueryAsync(Guid id)
        {
            var dbContext = dbContextProvider.GetDbContext();
            return await GetByIdQuery(dbContext, id);
        }

        public void AddEntities(IEnumerable<Department> entities)
        {
            var dbContext = dbContextProvider.GetDbContext();
            foreach (var entity in entities)
            {
                dbContext.Departments.Add(entity);
            }
            // This is synchronous and will process each insert in one batch.
            dbContext.SaveChanges();  
        }

        public async Task AddEntitiesInBatchesAsync(IEnumerable<Department> entities, int batchSize = 1000)
        {
            var dbContext = dbContextProvider.GetDbContext();
            var entityList = entities.ToList();
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                dbContext.Departments.AddRange(batch);
                // This asynchronously saves in batches
                await dbContext.SaveChangesAsync();  
            }
        }
        public async Task AddEntitiesUsingBulkInsertExtentionAsync(IEnumerable<Department> entities, int batchSize = 1000)
        {
            var options = new BulkConfig
            {
                SetOutputIdentity = true,
                BatchSize = batchSize
            };

            var dbContext = dbContextProvider.GetDbContext();
            await dbContext.BulkInsertAsync(entities, options);
        }
    }
}
