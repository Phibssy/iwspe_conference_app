using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Cosmos;
using Conference.Functions.Models;

namespace Conference.Functions.Services
{
    public class CosmosService
    {
        private readonly CosmosClient _client;
        private readonly CosmosDatabase _database;
        private readonly Container _programContainer;
        private readonly Container _registrationsContainer;

        public CosmosService()
        {
            var endpoint = Environment.GetEnvironmentVariable("COSMOS_DB_ENDPOINT") ?? throw new InvalidOperationException("COSMOS_DB_ENDPOINT not set");
            var key = Environment.GetEnvironmentVariable("COSMOS_DB_KEY") ?? throw new InvalidOperationException("COSMOS_DB_KEY not set");
            var dbName = Environment.GetEnvironmentVariable("COSMOS_DB_DATABASE") ?? "ConferenceDb";
            var programContainerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER_PROGRAM") ?? "Program";
            var regContainerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER_REGISTRATIONS") ?? "Registrations";

            _client = new CosmosClient(endpoint, key);
            _database = _client.CreateDatabaseIfNotExistsAsync(dbName).GetAwaiter().GetResult();
            _programContainer = _database.CreateContainerIfNotExistsAsync(programContainerName, "/id").GetAwaiter().GetResult();
            _registrationsContainer = _database.CreateContainerIfNotExistsAsync(regContainerName, "/id").GetAwaiter().GetResult();
        }

        public async Task AddRegistrationAsync(Registration reg)
        {
            if (string.IsNullOrWhiteSpace(reg.Id)) reg.Id = Guid.NewGuid().ToString();
            await _registrationsContainer.CreateItemAsync(reg, new PartitionKey(reg.Id));
        }

        public async Task<List<Registration>> GetRegistrationsAsync()
        {
            var sql = "SELECT * FROM c";
            var iterator = _registrationsContainer.GetItemQueryIterator<Registration>(new QueryDefinition(sql));
            var results = new List<Registration>();
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                results.AddRange(page.Resource);
            }
            return results;
        }

        public async Task UpsertProgramDocumentAsync(ProgramDocument program)
        {
            if (string.IsNullOrWhiteSpace(program.Id)) program.Id = Guid.NewGuid().ToString();
            await _programContainer.UpsertItemAsync(program, new PartitionKey(program.Id));
        }

        public async Task<List<ProgramDocument>> GetProgramAsync()
        {
            var sql = "SELECT * FROM c";
            var iterator = _programContainer.GetItemQueryIterator<ProgramDocument>(new QueryDefinition(sql));
            var results = new List<ProgramDocument>();
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                results.AddRange(page.Resource);
            }
            return results;
        }

        public async Task<List<EventItem>> GetBusScheduleAsync()
        {
            var programs = await GetProgramAsync();
            // Extract events of type bus from program documents
            var events = programs.SelectMany(p => p.Events ?? Enumerable.Empty<EventItem>())
                                 .Where(e => e.Type?.Equals("bus", StringComparison.OrdinalIgnoreCase) == true)
                                 .ToList();
            return events;
        }
    }
}