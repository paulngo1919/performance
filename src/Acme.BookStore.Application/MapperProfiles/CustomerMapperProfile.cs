using Acme.BookStore.Customers;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acme.BookStore.MapperProfiles
{
    public class CustomerMapperProfile:Profile
    {
        public CustomerMapperProfile()
        {
            CreateMap<Customer, CustomerDto>();
        }
    }
}
