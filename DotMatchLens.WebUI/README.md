# DotMatchLens WebUI

A Blazor Server web application that provides a user interface for the DotMatchLens football match prediction system.

## Overview

DotMatchLens.WebUI is built with:
- **Blazor Server** - Interactive server-rendered web UI framework
- **MudBlazor 8.15.0** - Material Design component library
- **.NET 10** - Latest .NET runtime
- **Aspire Service Defaults** - For distributed application support

## Features

### Dashboard
- Overview cards showing counts of teams, players, matches, and predictions
- Quick action buttons for common operations
- Real-time data updates

### Football Module
- **Teams Management**: View, filter by name/country, and create teams
- **Players Management**: View, filter by team, and create players
- **Matches Management**: View matches with date range filtering, create new matches
- **Competitions**: Sync competition data from football-data.org API

### Predictions Module
- **Match Predictions**: Generate AI-powered predictions for matches
- **AI Agent Query**: Natural language queries to the AI agent
- **Tools**:
  - Competition history lookup
  - Similar teams search (vector similarity)
  - Season statistics viewer
  - Competition search

### Workflows
- Trigger match prediction workflows
- View active workflows
- Batch prediction capabilities

## Architecture

### Services
The WebUI communicates with the API service through three HTTP client services:

- **FootballApiService**: Handles all football-related API calls (teams, players, matches, competitions)
- **PredictionsApiService**: Manages prediction generation, AI queries, and tool endpoints
- **WorkflowApiService**: Manages workflow triggering and monitoring

### Service Discovery
The WebUI uses Aspire's service discovery to automatically connect to the API service. The configuration is handled through the AppHost orchestrator.

## Configuration

The WebUI automatically discovers the API service URL through Aspire service discovery. Fallback configuration:

```csharp
var apiServiceUrl = builder.Configuration["services:apiservice:https:0"] 
    ?? builder.Configuration["services:apiservice:http:0"] 
    ?? "https://localhost:7001"; // Fallback for local development
```

## Running the Application

### Via AppHost (Recommended)
The recommended way to run the application is through the AppHost orchestrator:

```bash
cd DotMatchLens.AppHost
dotnet run
```

This will start all services including:
- PostgreSQL database (with pgvector)
- Redis cache
- Kafka message broker
- Ollama LLM container
- API Service
- WebUI

### Standalone (Development)
For development, you can run the WebUI standalone:

```bash
cd DotMatchLens.WebUI
dotnet run
```

Note: You'll need to manually configure the API service URL or ensure the API service is running.

## Project Structure

```
DotMatchLens.WebUI/
├── Components/
│   ├── App.razor              # Root component
│   ├── Routes.razor           # Routing configuration
│   ├── Layout/
│   │   ├── MainLayout.razor   # Main layout with MudBlazor components
│   │   └── NavMenu.razor      # Navigation menu
│   └── Pages/
│       ├── Home.razor         # Dashboard
│       ├── Football/
│       │   ├── Teams.razor
│       │   ├── Players.razor
│       │   ├── Matches.razor
│       │   └── Competitions.razor
│       ├── Predictions/
│       │   ├── Predictions.razor
│       │   └── AgentQuery.razor
│       ├── Workflows/
│       │   └── Workflows.razor
│       └── Tools/
│           └── Tools.razor
├── Services/
│   ├── FootballApiService.cs
│   ├── PredictionsApiService.cs
│   └── WorkflowApiService.cs
├── Program.cs                 # Application entry point
└── DotMatchLens.WebUI.csproj
```

## Dependencies

### NuGet Packages
- **MudBlazor** (8.15.0) - UI component library
- **Microsoft.AspNetCore.Components.Web** (via SDK)

### Project References
- **DotMatchLens.Core** - Shared contracts and interfaces
- **DotMatchLens.ServiceDefaults** - Aspire service defaults
- **DotMatchLens.Football** - Football module models and DTOs
- **DotMatchLens.Predictions** - Predictions module models and DTOs

## UI Features

### MudBlazor Components Used
- **MudLayout**, **MudAppBar**, **MudDrawer** - Layout structure
- **MudNavMenu**, **MudNavLink**, **MudNavGroup** - Navigation
- **MudDataGrid**, **MudTable** - Data display
- **MudDialog** - Modal dialogs
- **MudSnackbar** - Notifications
- **MudTextField**, **MudSelect**, **MudDatePicker** - Form inputs
- **MudButton**, **MudIconButton** - Actions
- **MudCard** - Content containers
- **MudProgressLinear**, **MudProgressCircular** - Loading states

### Theme
- Dark/Light theme toggle
- Default: Dark mode
- Customizable through MainLayout

## Error Handling

The application uses MudBlazor's Snackbar component for user-friendly error notifications:
- Success messages (green)
- Error messages (red)
- Warning messages (orange)
- Info messages (blue)

## Development Notes

### Adding New Pages
1. Create a new `.razor` file in the appropriate `Components/Pages/` subdirectory
2. Add `@page` directive with route
3. Add `@rendermode InteractiveServer` for server-side interactivity
4. Inject required services
5. Add navigation link in `NavMenu.razor`

### Best Practices
- Use MudBlazor components for consistency
- Implement loading states for async operations
- Use Snackbar for user feedback
- Follow existing patterns for service calls
- Handle errors gracefully with try-catch blocks

## Future Enhancements

Potential improvements:
- Real-time updates using SignalR
- Advanced data visualization with charts
- Export functionality for data
- User authentication and authorization
- Workflow visualization graphs
- Enhanced filtering and sorting options
- Pagination for large datasets
