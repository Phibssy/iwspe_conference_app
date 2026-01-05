using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Conference.Functions.Services;

namespace Conference.Functions
{
    public static class PromoteWaitlistFunction
    {
        private static readonly CosmosService _cosmos = new CosmosService();

        [Function("PromoteWaitlist")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registrations/promote/{eventId}")] HttpRequestData req, string eventId)
        {
            var principal = await AuthService.ValidateTokenAsync(req);
            if (principal == null) return AuthService.UnauthorizedResponse(req);

            var candidate = await _cosmos.GetFirstWaitlistedForEventAsync(eventId);
            if (candidate == null)
            {
                var notfound = req.CreateResponse(HttpStatusCode.NotFound);
                await notfound.WriteStringAsync(JsonSerializer.Serialize(new { message = "no waitlisted registrations" }));
                return notfound;
            }

            var sel = candidate.EventSelections.FirstOrDefault(s => s.EventId == eventId && s.Status == "waitlisted");
            if (sel == null)
            {
                var notfound2 = req.CreateResponse(HttpStatusCode.NotFound);
                await notfound2.WriteStringAsync(JsonSerializer.Serialize(new { message = "no waitlisted selections found" }));
                return notfound2;
            }

            sel.Status = "confirmed";
            sel.RegisteredAt = System.DateTime.UtcNow.ToString("o");

            await _cosmos.UpsertRegistrationAsync(candidate);

            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteStringAsync(JsonSerializer.Serialize(new { message = "promoted", id = candidate.Id, eventId = eventId }));
            return resp;
        }
    }
}