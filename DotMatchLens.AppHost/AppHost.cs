var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var footballDb = postgres.AddDatabase("footballdb");

// Add Redis for caching
var redis = builder.AddRedis("redis")
    .WithDataVolume();

var apiService = builder.AddProject<Projects.DotMatchLens_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(footballDb)
    .WithReference(redis)
    .WaitFor(footballDb)
    .WaitFor(redis);

builder.Build().Run();
