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
    public static class ProgramUploadFunction
    {
        private static readonly CosmosService _cosmos = new CosmosService();

        [Function("ProgramUpload")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "program/upload")] HttpRequestData req, FunctionContext executionContext)
        {
            var principal = await AuthService.ValidateTokenAsync(req);
            if (principal == null) return AuthService.UnauthorizedResponse(req);

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var program = JsonSerializer.Deserialize<ProgramDocument>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (program == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync(JsonSerializer.Serialize(new { error = "invalid program payload" }));
                return bad;
            }

            await _cosmos.UpsertProgramDocumentAsync(program);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(new { message = "program uploaded", id = program.Id }));
            return response;
        }
    }
}