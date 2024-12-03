using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Customers
{
    public class CustomerWithCacheRequestResultDto : PagedResultRequestDto
    {
        public bool ResetCache { get; set; } = false;
    }
}
