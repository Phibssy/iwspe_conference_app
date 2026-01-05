using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Conference.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Conference.Functions.Functions
{
    public class ProgramFunction
    {
        private readonly CosmosService _cosmos;
        public ProgramFunction(CosmosService cosmos)
        {
            _cosmos = cosmos;
        }

        [Function("GetProgram")]
        public async Task<HttpResponseData> GetProgram([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "program")] HttpRequestData req)
        {
            var programs = await _cosmos.GetProgramAsync();
            var events = programs.SelectMany(p => p.Events ?? Enumerable.Empty<Conference.Functions.Models.EventItem>())
                                  .Select(e => new { Id = e.Id, Title = e.Name, Capacity = (int?)e.Capacity })
                                  .ToList();
            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(events);
            return resp;
        }
    }
}