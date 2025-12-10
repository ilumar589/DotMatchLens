var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("pgvector/pgvector")
    .WithImageTag("pg16")
    .WithPgAdmin()
    .WithDataVolume();

var footballDb = postgres.AddDatabase("footballdb");

// Add Redis for caching
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Add Kafka for message-based communication between modules
var kafka = builder.AddKafka("kafka")
    .WithKafkaUI()
    .WithEnvironment("KAFKA_AUTO_CREATE_TOPICS_ENABLE", "true")
    .WithEnvironment("KAFKA_NUM_PARTITIONS", "1")
    .WithDataVolume();

// Add Ollama container for LLM predictions (not used for embeddings)
var ollama = builder.AddContainer("ollama", "ollama/ollama")
    .WithHttpEndpoint(port: 11434, targetPort: 11434, name: "ollama")
    .WithVolume("ollama-data", "/root/.ollama")
    .WithEnvironment("OLLAMA_HOST", "0.0.0.0");

var apiService = builder.AddProject<Projects.DotMatchLens_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(footballDb)
    .WithReference(redis)
    .WithReference(kafka)
    .WithEnvironment("OllamaAgent__Endpoint", ollama.GetEndpoint("ollama"))
    .WaitFor(footballDb)
    .WaitFor(redis)
    .WaitFor(kafka)
    .WaitFor(ollama);

// Add WebUI with reference to API service
builder.AddProject<Projects.DotMatchLens_WebUI>("webui")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
