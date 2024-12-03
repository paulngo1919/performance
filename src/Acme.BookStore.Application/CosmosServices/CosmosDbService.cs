using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Acme.BookStore.CosmosServices
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(string connectionString, string databaseName, string containerName)
        {
            var cosmosClientOptions = new CosmosClientOptions
            {

                HttpClientFactory = () =>
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                    };
                    return new HttpClient(handler);
                }
            };
            var cosmosClient = new CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", cosmosClientOptions);
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(string query)
        {
            var queryDefinition = new QueryDefinition(query);
            var queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition);

            var results = new List<T>();
            while (queryResultSetIterator.HasMoreResults)
            {
                try
                {
                    var response = await queryResultSetIterator.ReadNextAsync();

                    results.AddRange(response.ToList());
                }
                catch (Exception ex) { 
                
                }
            }

            return results;
        }
    }
}
