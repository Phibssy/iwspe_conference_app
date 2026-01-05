using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
// Remove the ambiguous using and be specific
using Microsoft.Azure.Functions.Worker.Http;  // Keep this
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Conference.Functions.Services
{
    public static class AuthService
    {
        private static readonly string TenantId = Environment.GetEnvironmentVariable("AZURE_AD_TENANT_ID");
        private static readonly string ClientId = Environment.GetEnvironmentVariable("AZURE_AD_CLIENT_ID");
        private static readonly string OpenIdConfigUrl = $"https://login.microsoftonline.com/{TenantId}/v2.0/.well-known/openid-configuration";
        private static readonly Microsoft.IdentityModel.Protocols.ConfigurationManager<OpenIdConnectConfiguration> _configManager = 
            new Microsoft.IdentityModel.Protocols.ConfigurationManager<OpenIdConnectConfiguration>(OpenIdConfigUrl, new OpenIdConnectConfigurationRetriever());

        public static async Task<ClaimsPrincipal> ValidateTokenAsync(Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        {
            if (!req.Headers.TryGetValues("Authorization", out var vals)) return null;
            var auth = vals.FirstOrDefault();
            if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer ")) return null;
            var token = auth.Substring("Bearer ".Length).Trim();
            try
            {
                var config = await _configManager.GetConfigurationAsync();
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = $"https://login.microsoftonline.com/{TenantId}/v2.0",
                    ValidAudiences = new[] { ClientId },
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Microsoft.Azure.Functions.Worker.Http.HttpResponseData UnauthorizedResponse(Microsoft.Azure.Functions.Worker.Http.HttpRequestData req)
        {
            var resp = req.CreateResponse(HttpStatusCode.Unauthorized);
            resp.WriteStringAsync("Unauthorized").GetAwaiter().GetResult();
            return resp;
        }
    }
}