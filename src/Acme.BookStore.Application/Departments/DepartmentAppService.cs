using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.Departments
{
    public class DepartmentAppService : ApplicationService
    {
        private readonly IDepartmentRepository departmentRepository;

        public DepartmentAppService(
            IDepartmentRepository departmentRepository
            )
        {
            this.departmentRepository = departmentRepository;
        }

        public bool AddDepartment(int Count)
        {
            List<Department> departments = new();
            for (int i = 0; i < Count; i++)
            {
                departments.Add(new Department
                {
                    Code = $"DP_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Name = $"DP_Name_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Region = "AddSaveChange"
                });
            }
            departmentRepository.AddEntities(departments);
            return true;
        }
        public async Task<bool> AddDepartmentInBatch(int Count, int BatchSize = 1000)
        {
            List<Department> departments = new();
            for (int i = 0; i < Count; i++)
            {
                departments.Add(new Department
                {
                    Code = $"DP_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Name = $"DP_Name_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Region = "BATCH"
                });
            }
            await departmentRepository.AddEntitiesInBatchesAsync(departments, BatchSize);
            return true;
        }
        public async Task<bool> AddDepartmentInBatchUsingExtention(int Count, int BatchSize = 1000)
        {
            List<Department> departments = new();
            for (int i = 0; i < Count; i++)
            {
                departments.Add(new Department(Guid.NewGuid())
                {
                    Code = $"DP_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Name = $"DP_Name_{DateTime.Now.ToString("ddMMyyyHHmmsss")}_{i}",
                    Region = "Extentions"
                });
            }
            await departmentRepository.AddEntitiesUsingBulkInsertExtentionAsync(departments, BatchSize);
            return true;
        }
    }
}
