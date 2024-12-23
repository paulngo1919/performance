using Acme.BookStore.Authors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Acme.BookStore.Departments
{
    public interface IDepartmentRepository : IRepository<Department, Guid>
    {
        Task<Department> FindIdAsync(Guid id);
        Task<Department> FindIdUsingCompiledQueryAsync(Guid id);
        void AddEntities(IEnumerable<Department> entities);
        Task AddEntitiesInBatchesAsync(IEnumerable<Department> entities, int batchSize = 1000);
        Task AddEntitiesUsingBulkInsertExtentionAsync(IEnumerable<Department> entities, int batchSize = 1000);
        Task AddOrUpdateUsingBulkExtentionAsync(IEnumerable<Department> entities, List<string> updateByProperties, int batchSize = 1000);
    }
}
