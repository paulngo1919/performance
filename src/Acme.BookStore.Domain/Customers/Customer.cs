using Acme.BookStore.Departments;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace Acme.BookStore.Customers
{
    public class Customer : Entity<Guid>
    {
        [MaxLength(50)]
        public string Code { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Title { get; set; }
        public string Position { get; set; }
        [MaxLength(11)]
        public string Phone { get; set; }
        public string Email { get; set; }

        public Guid? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
    }
}
