using DotMatchLens.WebUI.Components;
using DotMatchLens.WebUI.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure HTTP clients for API services
var apiServiceUrl = builder.Configuration["services:apiservice:https:0"] 
    ?? builder.Configuration["services:apiservice:http:0"] 
    ?? "https://localhost:7001"; // Fallback for local development

builder.Services.AddHttpClient<FootballApiService>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
});

builder.Services.AddHttpClient<PredictionsApiService>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
});

builder.Services.AddHttpClient<WorkflowApiService>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
