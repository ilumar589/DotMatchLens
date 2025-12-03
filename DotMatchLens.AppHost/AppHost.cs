var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var footballDb = postgres.AddDatabase("footballdb");

var apiService = builder.AddProject<Projects.DotMatchLens_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(footballDb)
    .WaitFor(footballDb);

builder.Build().Run();
