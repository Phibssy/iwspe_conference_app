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

            await _cosmos.AddRegistrationAsync(reg);

            var response = req.CreateResponse(HttpStatusCode.Created);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(new { message = "registration received", id = reg.Id }));
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