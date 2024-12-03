using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Customers
{
    public class CustomerRequestResultDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
