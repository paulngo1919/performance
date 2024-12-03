using Acme.BookStore.Departments;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Customers
{
    public class CustomerDto : EntityDto<Guid>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public DepartmentDto Department { get; set; }
    }
}
