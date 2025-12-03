# DotMatchLens

A modular monolith web API service for football data ingestion, storage, and AI-powered match predictions.

## Tech Stack

- **.NET 10** - Latest .NET runtime
- **Aspire** - Cloud-native application orchestration
- **PostgreSQL + pgvector** - Database with vector extension for AI embeddings
- **Entity Framework Core** - ORM with NoTracking default for read-heavy operations
- **Ollama** - Local AI model for predictions (compatible with Microsoft Agent Framework patterns)

## Architecture

DotMatchLens follows a modular monolith architecture with the following structure:

```
DotMatchLens/
├── DotMatchLens.AppHost/          # Aspire orchestration host
├── DotMatchLens.ServiceDefaults/  # Shared Aspire service configuration
├── DotMatchLens.ApiService/       # Web API entry point
├── DotMatchLens.Core/             # Shared types and global usings
├── DotMatchLens.Data/             # Entity Framework, PostgreSQL, pgvector
├── DotMatchLens.Football/         # Football data ingestion module
└── DotMatchLens.Predictions/      # AI prediction module
```

## Code Conventions

### Nullable Warnings as Errors
All nullable warnings are treated as errors. Use proper null checking and non-null assertions.

### Global Usings
Global usings are configured in `GlobalUsings.cs` files in each project.

### Prefer Concrete Types Over Interfaces
Avoid unnecessary abstractions. Use concrete types instead of interfaces when the abstraction provides no real benefit:
- Use `ImmutableArray<T>` instead of `IReadOnlyList<T>` or `IEnumerable<T>` for immutable collections
- Dependency injection works fine with concrete types
- Avoid creating interfaces solely for mocking in tests

```csharp
// Prefer this
public readonly record struct SeasonDto(
    int Id,
    ImmutableArray<string>? Stages);

// Instead of this
public readonly record struct SeasonDto(
    int Id,
    IReadOnlyList<string>? Stages);
```

### Testing Without Interfaces
Use these approaches for testing without introducing unnecessary interface abstractions:

1. **HttpMessageHandler for HTTP clients** - Inject custom handlers via `HttpClientFactory`
   ```csharp
   services.AddHttpClient<FootballDataApiClient>()
       .ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler());
   ```

2. **In-memory database for EF Core** - Use `UseInMemoryDatabase` for unit tests
   ```csharp
   var options = new DbContextOptionsBuilder<FootballDbContext>()
       .UseInMemoryDatabase("TestDb")
       .Options;
   ```

3. **WebApplicationFactory for integration tests** - Test actual API endpoints
   ```csharp
   public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
   ```

### Entity Framework Configuration
- **NoTracking by default** - Optimized for read-heavy operations
- **Projections to readonly record structs** - Efficient data transfer

```csharp
public readonly record struct TeamDto(
    Guid Id,
    string Name,
    string? Country,
    string? League);
```

### High-Performance Logging
Uses `LoggerMessage` source generators for compile-time optimized logging:

```csharp
public static partial class FootballLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Fetching teams with filter: {Filter}")]
    public static partial void LogFetchingTeams(ILogger logger, string? filter);
}
```

## API Endpoints

### Football Module (`/api/football`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/teams` | Get all teams with optional filtering |
| GET | `/teams/{id}` | Get a team by ID |
| POST | `/teams` | Create a new team |
| GET | `/players` | Get all players with optional team filtering |
| POST | `/players` | Create a new player |
| GET | `/matches` | Get matches within a date range |
| GET | `/matches/{id}` | Get a match by ID |
| POST | `/matches` | Create a new match |
| GET | `/matches/{id}/events` | Get events for a match |

### Predictions Module (`/api/predictions`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/generate` | Generate an AI prediction for a match |
| GET | `/match/{matchId}` | Get all predictions for a match |
| POST | `/query` | Query the AI agent with a custom question |

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for PostgreSQL via Aspire)
- Ollama (optional, for AI predictions)

### Running the Application

```bash
# Run with Aspire
cd DotMatchLens.AppHost
dotnet run
```

This will start:
- PostgreSQL database (with pgvector extension)
- pgAdmin for database management
- The API service

### Configuration

Configure Ollama in `appsettings.json`:

```json
{
  "OllamaAgent": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.2",
    "EmbeddingModel": "nomic-embed-text"
  }
}
```

## Database Schema

### Entities

- **Team** - Football team information
- **Player** - Player details with team association
- **Match** - Match scheduling and results
- **MatchEvent** - Events during a match (goals, cards, etc.)
- **MatchPrediction** - AI-generated predictions with vector embeddings

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Adding New Modules

1. Create a new class library project
2. Add reference to `DotMatchLens.Core`
3. Create `GlobalUsings.cs` for module-specific imports
4. Implement services and endpoints
5. Register the module in `DotMatchLens.ApiService`

## License

See [LICENSE](LICENSE) for details.