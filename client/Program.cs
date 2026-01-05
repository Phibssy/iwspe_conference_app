using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Conference.Client.Services;
using Microsoft.Authentication.WebAssembly.Msal;
using Microsoft.Authentication.WebAssembly.Msal.Models;
using Conference.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure MSAL authentication
var tenant = builder.Configuration["AzureAd:Authority"] ?? "https://login.microsoftonline.com/5805ebca-dc18-443f-b72c-7ed99b9934b5";
var clientId = builder.Configuration["AzureAd:ClientId"] ?? "e9608ffd-ca71-4d88-b08c-6ac1e94034a3";
var scope = builder.Configuration["AzureAd:DefaultAccessTokenScopes:0"] ?? $"api://{clientId}/access_as_user";

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.Authentication.Authority = tenant;
    options.ProviderOptions.Cache.CacheLocation = "localStorage";
    options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
});

// Register HttpClient
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Register your ApiService
builder.Services.AddScoped<ApiService>();

// Register root component
builder.RootComponents.Add<Conference.Client.Shared.App>("#app");

await builder.Build().RunAsync();