using System.ComponentModel;
using System.Reflection;
using DotMatchLens.Predictions.Logging;
using DotMatchLens.Predictions.Models;
using DotMatchLens.Predictions.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace DotMatchLens.Predictions.Agents;

/// <summary>
/// Football agent service using Microsoft Agent Framework with Ollama's OpenAI-compatible endpoint.
/// </summary>
public sealed class FootballAgentService
{
    private readonly ChatClientAgent _agent;
    private readonly ILogger<FootballAgentService> _logger;
    private readonly string _modelVersion;

    private const string AgentInstructions = """
        You are an expert football data analyst with access to a comprehensive database of football competitions, 
        teams, and seasons. You can help users with:
        
        - Finding historical competition data and past winners
        - Searching for teams by name, country, or other characteristics
        - Looking up season statistics and current standings
        - Semantic search across competitions
        
        When answering questions, use the available tools to retrieve accurate data from the database.
        Provide clear, concise answers based on the data you find. If a tool returns an error,
        explain the issue to the user and suggest alternatives.
        
        Be helpful, accurate, and professional in your responses.
        """;

    public FootballAgentService(
        OllamaAgentOptions options,
        FootballDataTools tools,
        ILogger<FootballAgentService> logger,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _modelVersion = options.Model;

        // Create OpenAI client pointing to Ollama's OpenAI-compatible endpoint
        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(options.Endpoint)
        };

        var credential = new ApiKeyCredential(options.ApiKey);
        var chatClient = new OpenAI.Chat.ChatClient(options.Model, credential, clientOptions);

        // Create AI tools from FootballDataTools methods with [Description] attributes
        var aiTools = CreateAIToolsFromInstance(tools);

        // Create the agent using Microsoft Agent Framework
        _agent = chatClient.CreateAIAgent(
            instructions: AgentInstructions,
            name: "FootballDataAgent",
            description: "Football data analysis agent with database access",
            tools: aiTools,
            loggerFactory: loggerFactory);

        PredictionLogMessages.LogAgentInvoked(_logger, _modelVersion);
    }

    /// <summary>
    /// Query the football agent with a user question.
    /// </summary>
    /// <param name="query">The user's question or query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Agent response with the answer.</returns>
    public async Task<AgentResponse> QueryAsync(string query, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        PredictionLogMessages.LogAgentInvoked(_logger, _modelVersion);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create a new thread for this conversation
            var thread = _agent.GetNewThread();

            // Run the agent with the user's query
            var response = await _agent.RunAsync(query, thread, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            stopwatch.Stop();
            PredictionLogMessages.LogAgentResponseReceived(_logger, stopwatch.ElapsedMilliseconds);

            // Extract the response text from the agent's messages
            var responseText = ExtractResponseText(response);

            return new AgentResponse(responseText, _modelVersion, null);
        }
        catch (Exception ex)
        {
            PredictionLogMessages.LogAgentQueryError(_logger, ex.Message, ex);
            return new AgentResponse(
                "Sorry, I couldn't process your query at this time. Please try again later.",
                _modelVersion,
                null);
        }
    }

    /// <summary>
    /// Creates a list of AITools from an object instance by finding methods with [Description] attributes.
    /// </summary>
    private static List<AITool> CreateAIToolsFromInstance(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var aiTools = new List<AITool>();
        var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null)
            {
                var aiFunction = AIFunctionFactory.Create(method, instance);
                aiTools.Add(aiFunction);
            }
        }

        return aiTools;
    }

    /// <summary>
    /// Extracts the response text from agent run response.
    /// </summary>
    private static string ExtractResponseText(AgentRunResponse response)
    {
        if (response.Messages.Count == 0)
        {
            return "No response generated.";
        }

        // Get the last assistant message
        var lastMessage = response.Messages
            .LastOrDefault(m => m.Role == ChatRole.Assistant);

        if (lastMessage is null)
        {
            return "No response generated.";
        }

        // Extract text content from the message
        var textContent = lastMessage.Contents
            .OfType<TextContent>()
            .FirstOrDefault();

        return textContent?.Text ?? "No response generated.";
    }
}
