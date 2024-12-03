using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Acme.BookStore.Departments
{
    public class Department : Entity<Guid>
    {
        public Department()
        {
            
        }
        // Constructor that allows setting Id explicitly
        public Department(Guid id)
        {
            Id = id; // This will set the Id explicitly
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
    }
}
