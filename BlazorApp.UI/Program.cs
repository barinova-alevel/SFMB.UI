using System.Globalization;
using BlazorApp.UI.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp.UI.Auth;
using BlazorApp.UI.Auth.Services;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("uk-UA");

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration.GetValue<string>("API_URL") 
                 ?? builder.Configuration.GetConnectionString("ApiBaseUrl") //optional fallback to appsettings
                 ?? throw new InvalidOperationException("API_URL is not configured.");

//var apiBaseUrl = builder.Configuration.GetConnectionString("ApiBaseUrl")
//                 ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");
// --- END: API URL CONFIGURATION ---


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication and authorization services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Register custom authentication services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/data/protection"))
    .SetApplicationName("goldfish-app");

var blazorDomain = "https://sfmb-ui.https://goldfish-app-j6a9p.ondigitalocean.app/";
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins(blazorDomain)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

//app.UseStatusCodePagesWithRedirects("/");

//uncomment after testing!!!
//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
