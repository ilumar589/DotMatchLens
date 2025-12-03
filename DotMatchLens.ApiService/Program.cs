using DotMatchLens.Data.Extensions;
using DotMatchLens.Football;
using DotMatchLens.Predictions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add database
builder.AddFootballDatabase();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add modules
builder.Services.AddFootballModule(builder.Configuration);
builder.Services.AddPredictionsModule();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "DotMatchLens API is running. Navigate to /openapi/v1.json for API documentation.");

// Map module endpoints
app.UseFootballModule();
app.UsePredictionsModule();

app.MapDefaultEndpoints();

app.Run();
