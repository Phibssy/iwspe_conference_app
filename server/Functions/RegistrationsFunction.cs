using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Conference.Functions.Models;
using Conference.Functions.Services;

namespace Conference.Functions
{
    public static class RegistrationsFunction
    {
        private static readonly CosmosService _cosmos = new CosmosService();

        [Function("Register")]
        public static async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Function, "post", Route = "registrations")] HttpRequestData req, FunctionContext ctx)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var reg = JsonSerializer.Deserialize<Registration>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (!Conference.Functions.Services.ValidationService.TryValidateRegistration(reg, out var validationError))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync(JsonSerializer.Serialize(new { error = validationError }));
                return bad;
            }

            // Map legacy SelectedEvents to EventSelections if present
            if ((reg.EventSelections == null || reg.EventSelections.Length == 0) && reg.SelectedEvents != null && reg.SelectedEvents.Length > 0)
            {
                reg.EventSelections = reg.SelectedEvents.Select(e => new Models.EventSelection { EventId = e }).ToArray();
            }

            var enforce = (Environment.GetEnvironmentVariable("ENFORCE_CAPACITY") ?? "false").ToLower() == "true";

            // Load events to get capacities
            var programs = await _cosmos.GetProgramAsync();
            var events = programs.SelectMany(p => p.Events ?? Enumerable.Empty<Models.EventItem>()).ToDictionary(e => e.Id ?? e.Name, e => e);

            var rejectedEvents = new List<string>();

            foreach (var sel in reg.EventSelections ?? Array.Empty<Models.EventSelection>())
            {
                if (string.IsNullOrWhiteSpace(sel.EventId)) continue;
                if (!events.TryGetValue(sel.EventId, out var evt))
                {
                    // unknown event - reject
                    rejectedEvents.Add(sel.EventId);
                    continue;
                }

                var confirmed = await _cosmos.CountConfirmedForEventAsync(sel.EventId);
                var waitlisted = await _cosmos.CountWaitlistedForEventAsync(sel.EventId);
                var status = CapacityManager.DecideStatus(confirmed, waitlisted, evt.Capacity, evt.WaitlistCapacity, enforce);
                if (status == "rejected")
                {
                    rejectedEvents.Add(sel.EventId);
                }
                else
                {
                    sel.Status = status;
                    sel.RegisteredAt = DateTime.UtcNow.ToString("o");
                }
            }

            if (rejectedEvents.Any())
            {
                var conflict = req.CreateResponse(HttpStatusCode.Conflict);
                await conflict.WriteStringAsync(JsonSerializer.Serialize(new { error = "events full", events = rejectedEvents }));
                return conflict;
            }

            // set creation timestamp
            if (string.IsNullOrWhiteSpace(reg.CreatedAt)) reg.CreatedAt = DateTime.UtcNow.ToString("o");

            await _cosmos.UpsertRegistrationAsync(reg);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(new { message = "registration received", id = reg.Id, selections = reg.EventSelections }));
            return response;
        }

        [Function("GetRegistrations")]
        public static async Task<HttpResponseData> GetRegistrations([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations")] HttpRequestData req)
        {
            var principal = await AuthService.ValidateTokenAsync(req);
            if (principal == null) return AuthService.UnauthorizedResponse(req);

            var regs = await _cosmos.GetRegistrationsAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(regs));
            return response;
        }
    }
}