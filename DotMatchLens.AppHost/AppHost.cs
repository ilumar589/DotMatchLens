var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var footballDb = postgres.AddDatabase("footballdb");

// Use /alive for Aspire orchestration - checks only app responsiveness
// Use /ready manually to verify all dependencies are available
var apiService = builder.AddProject<Projects.DotMatchLens_ApiService>("apiservice")
    .WithHttpHealthCheck("/alive")
    .WithReference(footballDb)
    .WaitFor(footballDb);

builder.Build().Run();
