using DotMatchLens.Predictions.Observability;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for workflow metrics.
/// </summary>
public sealed class WorkflowMetricsTests : IDisposable
{
    private readonly WorkflowMetrics _metrics;

    public WorkflowMetricsTests()
    {
        _metrics = new WorkflowMetrics();
    }

    [Fact]
    public void RecordWorkflowStarted_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordWorkflowStarted("match_prediction");
    }

    [Fact]
    public void RecordWorkflowCompleted_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordWorkflowCompleted("match_prediction", 1500.5);
    }

    [Fact]
    public void RecordWorkflowFailed_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordWorkflowFailed("match_prediction", "timeout");
    }

    [Fact]
    public void RecordAgentInvocation_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordAgentInvocation("ollama", "llama3.2");
    }

    [Fact]
    public void RecordAgentResponse_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordAgentResponse("ollama", "llama3.2", 250.5);
    }

    [Fact]
    public void RecordAgentError_ShouldNotThrow()
    {
        // Act & Assert
        _metrics.RecordAgentError("ollama", "connection_error");
    }

    [Fact]
    public void RecordPredictionGenerated_ShouldNotThrow()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var confidence = 0.85f;

        // Act & Assert
        _metrics.RecordPredictionGenerated(matchId, confidence);
    }

    [Fact]
    public void MultipleMetrics_ShouldNotThrow()
    {
        // Arrange
        var matchId = Guid.NewGuid();

        // Act & Assert - Record a complete workflow
        _metrics.RecordWorkflowStarted("match_prediction");
        _metrics.RecordAgentInvocation("ollama", "llama3.2");
        _metrics.RecordAgentResponse("ollama", "llama3.2", 200.0);
        _metrics.RecordPredictionGenerated(matchId, 0.9f);
        _metrics.RecordWorkflowCompleted("match_prediction", 1000.0);
    }

    public void Dispose()
    {
        _metrics.Dispose();
    }
}
