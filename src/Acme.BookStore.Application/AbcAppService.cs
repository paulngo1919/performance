using Acme.BookStore.Customers;
using Acme.BookStore.Departments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace Acme.BookStore
{
    public class AbcAppService : ApplicationService
    {
        private readonly IRepository<Customer, Guid> repository;
        private readonly IRepository<Department, Guid> repositoryDepartment;
        private readonly IDepartmentRepository departmentRepository;
        private readonly IDistributedCache<List<Customer>> cache;
        private readonly IObjectMapper objectMapper;

        public AbcAppService(
            IRepository<Customer, Guid> repository,
            IRepository<Department, Guid> repositoryDepartment,
            IDepartmentRepository departmentRepository,
            IDistributedCache<List<Customer>> cache,
            IObjectMapper objectMapper)
        {
            this.repository = repository;
            this.repositoryDepartment = repositoryDepartment;
            this.departmentRepository = departmentRepository;
            this.cache = cache;
            this.objectMapper = objectMapper;
        }

        #region DB index
        /// <summary>
        /// API return optimize query fields, có điều kiện query theo column Code
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public async Task<List<CustomerDto>> GetCustomerListByCode(CustomerRequestResultDto input)
        {
            // Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.WithDetailsAsync(x => x.Department);
            var mappedQuery = queryable.Where(d => d.Code == input.Keyword).Skip(0).Take(input.MaxResultCount).Select(d => new CustomerDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Department = new Departments.DepartmentDto
                {
                    Id = d.Department.Id,
                    Name = d.Department.Name
                }
            });
            var datas = await AsyncExecuter.ToListAsync(mappedQuery);
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerListByCode] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerListByCode] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");

            return datas;
        }




        #endregion

        #region Compilied Query
        public async Task<Customer> GetCustomerById(Guid id)
        {
            var queryable = await repository.GetQueryableAsync();
            var mappedQuery = queryable.Where(d => d.Id == id);
            var data = await AsyncExecuter.FirstOrDefaultAsync(mappedQuery);
            if (data.DepartmentId.HasValue)
                data.Department = await departmentRepository.FindIdAsync(data.DepartmentId.Value);
            return data;
        }
        public async Task<Customer> GetCustomerByIdCompiledQuery(Guid id)
        {
            var queryable = await repository.GetQueryableAsync();
            var mappedQuery = queryable.Where(d => d.Id == id);
            var data = await AsyncExecuter.FirstOrDefaultAsync(mappedQuery);
            if (data.DepartmentId.HasValue)
                data.Department = await departmentRepository.FindIdUsingCompiledQueryAsync(data.DepartmentId.Value);
            return data;
        }
        #endregion

        #region cache
        /// <summary>
        /// API return raw entity with multiple database call in loop using cache data
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerListMultileDatabaseCallCache(CustomerWithCacheRequestResultDto input)
        {
            string cachekey = $"demo_{input.MaxResultCount}";
            if (input.ResetCache) await cache.RemoveAsync(cachekey);

            return await cache.GetOrAddAsync(cachekey, async () =>
            {
                // Record the memory usage before execution
                long memoryBefore = GC.GetTotalMemory(true);

                Stopwatch stopwatch = Stopwatch.StartNew();
                stopwatch.Start();
                var queryable = await repository.GetQueryableAsync();
                queryable = queryable.Skip(0).Take(input.MaxResultCount);
                var datas = await AsyncExecuter.ToListAsync(queryable);
                foreach (var item in datas)
                {
                    item.Department = await repositoryDepartment.FirstOrDefaultAsync(d => d.Id == item.DepartmentId);
                }
                stopwatch.Stop();
                Console.WriteLine($"[GetCustomerListMultileDatabaseCall] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
                // Record the memory usage after execution
                long memoryAfter = GC.GetTotalMemory(true);
                Console.WriteLine($"[GetCustomerListMultileDatabaseCall] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");

                return datas;

            });

        }
        #endregion
        #region Reduce Database Call

        /// <summary>
        /// API return raw entity with multiple database call in loop
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerListMultileDatabaseCall(LimitedResultRequestDto input)
        {
            // Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.GetQueryableAsync();
            queryable = queryable.Skip(0).Take(input.MaxResultCount);
            var datas = await AsyncExecuter.ToListAsync(queryable);
            foreach (var item in datas)
            {
                item.Department = await repositoryDepartment.FirstOrDefaultAsync(d => d.Id == item.DepartmentId);
            }
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerListMultileDatabaseCall] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerListMultileDatabaseCall] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");
            return datas;
        }
        /// <summary>
        /// API return raw entity with multiple database call enhance using linq first
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerListMultileDatabaseCallEnhanceFirstInLoop(LimitedResultRequestDto input)
        {
            // Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.GetQueryableAsync();
            queryable = queryable.Skip(0).Take(input.MaxResultCount);
            var datas = await AsyncExecuter.ToListAsync(queryable);

            var departmentIds = datas.Select(d => d.DepartmentId).Distinct().ToList();
            var departments = (await repositoryDepartment.GetQueryableAsync()).Where(d => departmentIds.Contains(d.Id)).ToList();

            foreach (var item in datas)
            {
                item.Department = item.DepartmentId.HasValue ? departments.FirstOrDefault(d => d.Id == item.DepartmentId.Value) : default;
            }
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerListMultileDatabaseCallEnhanceLinqFirst] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerListMultileDatabaseCallEnhanceLinqFirst] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");
            return datas;
        }

        /// <summary>
        /// API return raw entity with multiple database call enhance using dictionary map
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerListMultileDatabaseCallEnhanceMapDic(LimitedResultRequestDto input)
        {
            // Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.GetQueryableAsync();
            queryable = queryable.Skip(0).Take(input.MaxResultCount);
            var datas = await AsyncExecuter.ToListAsync(queryable);

            var departmentIds = datas.Select(d => d.DepartmentId).Distinct().ToList();
            var departmentDic = (await repositoryDepartment.GetQueryableAsync()).Where(d => departmentIds.Contains(d.Id)).ToDictionary(d => d.Id, m => m);

            foreach (var item in datas)
            {
                item.Department = item.DepartmentId.HasValue && departmentDic.TryGetValue(item.DepartmentId.Value, out var d) ? d : default;
            }
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerListMultileDatabaseCallEnhance] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerListMultileDatabaseCallEnhance] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");
            return datas;
        }


        #endregion
        /// <summary>
        /// API return raw entity
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<Customer>> GetCustomerList(LimitedResultRequestDto input)
        {
            // Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.WithDetailsAsync(x => x.Department);
            queryable = queryable.Skip(0).Take(input.MaxResultCount);
            var datas = await AsyncExecuter.ToListAsync(queryable);
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerList] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerList] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");
            return datas;
        }

        /// <summary>
        /// API return optimize query fields
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public async Task<List<CustomerDto>> GetCustomerListOptimizeQuery(LimitedResultRequestDto input)
        {// Record the memory usage before execution
            long memoryBefore = GC.GetTotalMemory(true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var queryable = await repository.WithDetailsAsync(x => x.Department);
            var mappedQuery = queryable.Skip(0).Take(input.MaxResultCount).Select(d => new CustomerDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Department = new Departments.DepartmentDto
                {
                    Id = d.Department.Id,
                    Name = d.Department.Name
                }
            });
            var datas = await AsyncExecuter.ToListAsync(mappedQuery);
            stopwatch.Stop();
            Console.WriteLine($"[GetCustomerListOptimizeQuery] Total times in miliseconds: {stopwatch.ElapsedMilliseconds}");
            // Record the memory usage after execution
            long memoryAfter = GC.GetTotalMemory(true);
            Console.WriteLine($"[GetCustomerListOptimizeQuery] Memory usage (before: {memoryBefore} bytes, after: {memoryAfter} bytes, difference: {memoryAfter - memoryBefore} bytes)");

            return datas;
        }
    }
}
