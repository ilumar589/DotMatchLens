using DotMatchLens.Core.Services;
using DotMatchLens.Data.Extensions;
using DotMatchLens.Football;
using DotMatchLens.Football.Consumers;
using DotMatchLens.Predictions;
using DotMatchLens.Predictions.Consumers;
using DotMatchLens.Predictions.Sagas;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add database
builder.AddFootballDatabase();

// Add database migrations
builder.AddDatabaseMigrations();

// Add Redis caching
builder.AddRedisClient("redis");

// Add cache service
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add modules
builder.Services.AddFootballModule(builder.Configuration);
builder.Services.AddPredictionsModule(builder.Configuration);

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<CompetitionSyncConsumer>();
    x.AddConsumer<MatchPredictionConsumer>();

    // Add saga state machine with in-memory repository
    x.AddSagaStateMachine<PredictionSaga, PredictionSagaState>()
        .InMemoryRepository();

    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        // Get RabbitMQ connection from Aspire service discovery
        var configuration = context.GetRequiredService<IConfiguration>();
        var rabbitConnection = configuration.GetConnectionString("rabbitmq");
        
        var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMQSetup");
        logger.LogInformation("RabbitMQ connection string: {ConnectionString}", rabbitConnection);
        
        if (!string.IsNullOrEmpty(rabbitConnection))
        {
            cfg.Host(new Uri(rabbitConnection));
        }
        else
        {
            // Fallback for local development without Aspire
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        // Configure endpoints automatically
        cfg.ConfigureEndpoints(context);
        
        // Configure message retry
        cfg.UseMessageRetry(r => r.Intervals(100, 200, 500, 1000, 2000));
        
        // Configure error handling
        cfg.UseInMemoryOutbox(context);
    });
});

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
