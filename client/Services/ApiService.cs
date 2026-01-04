using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Conference.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IAccessTokenProvider _tokenProvider;

        public ApiService(HttpClient http, IAccessTokenProvider tokenProvider)
        {
            _http = http;
            _tokenProvider = tokenProvider;
        }

        private async Task<bool> AttachTokenAsync()
        {
            var result = await _tokenProvider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
                return true;
            }
            return false;
        }

        public async Task<HttpResponseMessage> GetRegistrationsAsync()
        {
            if (!await AttachTokenAsync()) return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            return await _http.GetAsync("api/registrations");
        }

        public async Task<HttpResponseMessage> UploadProgramAsync(object program)
        {
            if (!await AttachTokenAsync()) return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            var content = new StringContent(JsonSerializer.Serialize(program));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await _http.PostAsync("api/program/upload", content);
        }
    }
}