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

// Configure MassTransit with Kafka
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<CompetitionSyncConsumer>();
    x.AddConsumer<MatchPredictionConsumer>();

    // Add saga state machine
    x.AddSagaStateMachine<PredictionSaga, PredictionSagaState>()
        .InMemoryRepository(); // Use in-memory for now, can switch to Entity Framework or Redis later

    // Configure Kafka
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    // Add Kafka riders for pub/sub
    x.AddRider(rider =>
    {
        rider.AddConsumer<CompetitionSyncConsumer>();
        rider.AddConsumer<MatchPredictionConsumer>();

        rider.UsingKafka((context, k) =>
        {
            // Get Kafka connection from Aspire service discovery
            var kafkaConnection = builder.Configuration.GetConnectionString("kafka") ?? "localhost:9092";
            k.Host(kafkaConnection);

            k.TopicEndpoint<DotMatchLens.Core.Contracts.CompetitionSyncRequested>("competition-sync", "dotmatchlens-group", e =>
            {
                e.ConfigureConsumer<CompetitionSyncConsumer>(context);
            });

            k.TopicEndpoint<DotMatchLens.Core.Contracts.MatchPredictionRequested>("match-prediction", "dotmatchlens-group", e =>
            {
                e.ConfigureConsumer<MatchPredictionConsumer>(context);
            });
        });
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
