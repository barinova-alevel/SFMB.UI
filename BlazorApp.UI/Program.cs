using System.Globalization;
using BlazorApp.UI.Components;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("uk-UA");

var builder = WebApplication.CreateBuilder(args);

// --- API URL CONFIGURATION: Read from Configuration ---
// It will try to read from: 
// 1. Environment Variable: ConnectionStrings__ApiBaseUrl 
// 2. appsettings.json: "ConnectionStrings": { "ApiBaseUrl": "..." }
// 

var apiBaseUrl = builder.Configuration.GetValue<string>("API_URL") // <-- Read the environment variable directly
                 ?? builder.Configuration.GetConnectionString("ApiBaseUrl") // <-- Optional fallback to appsettings
                 ?? throw new InvalidOperationException("API_URL is not configured.");

//var apiBaseUrl = builder.Configuration.GetConnectionString("ApiBaseUrl")
//                 ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");
// --- END: API URL CONFIGURATION ---


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/");

//uncomment after testing!!!
//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
