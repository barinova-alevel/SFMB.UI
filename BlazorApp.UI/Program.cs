using System.Globalization;
using BlazorApp.UI.Components;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("uk-UA");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://sfmbwebapi-dfb4hkgjeeh7cnbt.polandcentral-01.azurewebsites.net");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/");

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
