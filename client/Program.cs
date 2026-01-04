using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Configure MSAL authentication
var tenant = builder.Configuration["AzureAd:Authority"] ?? $"https://login.microsoftonline.com/5805ebca-dc18-443f-b72c-7ed99b9934b5";
var clientId = builder.Configuration["AzureAd:ClientId"] ?? "e9608ffd-ca71-4d88-b08c-6ac1e94034a3";
var scope = builder.Configuration["AzureAd:DefaultAccessTokenScopes:0"] ?? $"api://{clientId}/access_as_user";

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.Authentication.Authority = tenant;
    options.ProviderOptions.ClientId = clientId;
    // Request the API scope for backend calls
    options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
});

// HttpClient that can call the API; protected calls should request token via IAccessTokenProvider
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();