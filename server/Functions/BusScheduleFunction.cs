using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Conference.Functions.Services;

namespace Conference.Functions
{
    public static class BusScheduleFunction
    {
        private static readonly CosmosService _cosmos = new CosmosService();

        [Function("GetBusSchedule")]
        public static async Task<HttpResponseData> GetBusSchedule([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "busschedule")] HttpRequestData req)
        {
            // Public endpoint — bus schedule is typically public. No auth required.
            var events = await _cosmos.GetBusScheduleAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(events));
            return response;
        }
    }
}