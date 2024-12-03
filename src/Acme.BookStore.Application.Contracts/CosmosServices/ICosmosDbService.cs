using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Acme.BookStore.CosmosServices
{
    public interface ICosmosDbService
    {
        Task<IEnumerable<T>> GetItemsAsync<T>(string query);
    }
}
