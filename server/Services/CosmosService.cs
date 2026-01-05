using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Conference.Functions.Models;

namespace Conference.Functions.Services
{
    public class CosmosService
    {
        private readonly CosmosClient _client;
        private readonly Database _database;
        private readonly Container _programContainer;
        private readonly Container _registrationsContainer;

        public CosmosService()
        {
            var endpoint = Environment.GetEnvironmentVariable("COSMOS_DB_ENDPOINT") ?? throw new InvalidOperationException("COSMOS_DB_ENDPOINT not set");
            var key = Environment.GetEnvironmentVariable("COSMOS_DB_KEY") ?? throw new InvalidOperationException("COSMOS_DB_KEY not set");
            var dbName = Environment.GetEnvironmentVariable("COSMOS_DB_DATABASE") ?? "ConferenceDb";
            var programContainerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER_PROGRAM") ?? "Program";
            var regContainerName = Environment.GetEnvironmentVariable("COSMOS_DB_CONTAINER_REGISTRATIONS") ?? "Registrations";
            
            // For local emulator, disable SSL validation
            var options = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                HttpClientFactory = () => new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                })
            };
            
            _client = new CosmosClient(endpoint, key, options);
            
            var dbResponse = _client.CreateDatabaseIfNotExistsAsync(dbName).GetAwaiter().GetResult();
            _database = dbResponse.Database;
            
            var programContainerResponse = _database.CreateContainerIfNotExistsAsync(programContainerName, "/id").GetAwaiter().GetResult();
            _programContainer = programContainerResponse.Container;
            
            var registrationsContainerResponse = _database.CreateContainerIfNotExistsAsync(regContainerName, "/id").GetAwaiter().GetResult();
            _registrationsContainer = registrationsContainerResponse.Container;
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

        public async Task<int> CountConfirmedForEventAsync(string eventId)
        {
            var sql = "SELECT VALUE COUNT(1) FROM c JOIN s IN c.EventSelections WHERE s.EventId = @eventId AND s.Status = 'confirmed'";
            var iterator = _registrationsContainer.GetItemQueryIterator<int>(new QueryDefinition(sql).WithParameter("@eventId", eventId));
            var count = 0;
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                count += page.Resource.FirstOrDefault();
            }
            return count;
        }

        public async Task<int> CountWaitlistedForEventAsync(string eventId)
        {
            var sql = "SELECT VALUE COUNT(1) FROM c JOIN s IN c.EventSelections WHERE s.EventId = @eventId AND s.Status = 'waitlisted'";
            var iterator = _registrationsContainer.GetItemQueryIterator<int>(new QueryDefinition(sql).WithParameter("@eventId", eventId));
            var count = 0;
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                count += page.Resource.FirstOrDefault();
            }
            return count;
        }

        public async Task<Registration> GetFirstWaitlistedForEventAsync(string eventId)
        {
            // For simplicity, pull all registrations and find the earliest created with a waitlisted entry
            var regs = await GetRegistrationsAsync();
            var waitlisted = regs.Where(r => r.EventSelections != null && r.EventSelections.Any(s => s.EventId == eventId && s.Status == "waitlisted"))
                                 .OrderBy(r => DateTime.TryParse(r.CreatedAt, out var d) ? d : DateTime.MaxValue)
                                 .FirstOrDefault();
            return waitlisted;
        }

        public async Task UpsertRegistrationAsync(Registration reg)
        {
            if (string.IsNullOrWhiteSpace(reg.Id)) reg.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(reg.CreatedAt)) reg.CreatedAt = DateTime.UtcNow.ToString("o");
            await _registrationsContainer.UpsertItemAsync(reg, new PartitionKey(reg.Id));
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