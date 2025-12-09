using DotMatchLens.Predictions.Logging;
using Microsoft.Extensions.Logging;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for workflow log messages.
/// </summary>
public sealed class WorkflowLogMessagesTests
{
    [Fact]
    public void LogWorkflowStarted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var workflowType = "match_prediction";
        var correlationId = Guid.NewGuid();

        // Act & Assert
        WorkflowLogMessages.LogWorkflowStarted(logger, workflowType, correlationId);
    }

    [Fact]
    public void LogWorkflowCompleted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var workflowType = "match_prediction";
        var correlationId = Guid.NewGuid();
        var durationMs = 1500L;

        // Act & Assert
        WorkflowLogMessages.LogWorkflowCompleted(logger, workflowType, correlationId, durationMs);
    }

    [Fact]
    public void LogWorkflowFailed_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var workflowType = "match_prediction";
        var correlationId = Guid.NewGuid();
        var errorMessage = "Test error";
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        WorkflowLogMessages.LogWorkflowFailed(logger, workflowType, correlationId, errorMessage, exception);
    }

    [Fact]
    public void LogWorkflowStepStarted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var stepName = "fetch_data";
        var workflowType = "match_prediction";

        // Act & Assert
        WorkflowLogMessages.LogWorkflowStepStarted(logger, stepName, workflowType);
    }

    [Fact]
    public void LogWorkflowStepCompleted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var stepName = "fetch_data";
        var workflowType = "match_prediction";
        var durationMs = 250L;

        // Act & Assert
        WorkflowLogMessages.LogWorkflowStepCompleted(logger, stepName, workflowType, durationMs);
    }

    [Fact]
    public void LogWorkflowStepFailed_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var stepName = "fetch_data";
        var workflowType = "match_prediction";
        var errorMessage = "Step failed";
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        WorkflowLogMessages.LogWorkflowStepFailed(logger, stepName, workflowType, errorMessage, exception);
    }

    [Fact]
    public void LogConsumerProcessing_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var messageType = "MatchPredictionRequested";
        var correlationId = Guid.NewGuid();

        // Act & Assert
        WorkflowLogMessages.LogConsumerProcessing(logger, messageType, correlationId);
    }

    [Fact]
    public void LogConsumerCompleted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var messageType = "MatchPredictionRequested";
        var correlationId = Guid.NewGuid();
        var durationMs = 500L;

        // Act & Assert
        WorkflowLogMessages.LogConsumerCompleted(logger, messageType, correlationId, durationMs);
    }

    [Fact]
    public void LogConsumerFailed_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var messageType = "MatchPredictionRequested";
        var correlationId = Guid.NewGuid();
        var errorMessage = "Consumer failed";
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        WorkflowLogMessages.LogConsumerFailed(logger, messageType, correlationId, errorMessage, exception);
    }

    [Fact]
    public void LogPublishingMessage_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var messageType = "MatchPredictionCompleted";
        var correlationId = Guid.NewGuid();

        // Act & Assert
        WorkflowLogMessages.LogPublishingMessage(logger, messageType, correlationId);
    }

    [Fact]
    public void LogWorkflowStateTransition_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var fromState = "Pending";
        var toState = "Processing";
        var workflowType = "match_prediction";

        // Act & Assert
        WorkflowLogMessages.LogWorkflowStateTransition(logger, fromState, toState, workflowType);
    }

    [Fact]
    public void LogBatchWorkflowStarted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var batchSize = 10;
        var correlationId = Guid.NewGuid();

        // Act & Assert
        WorkflowLogMessages.LogBatchWorkflowStarted(logger, batchSize, correlationId);
    }

    [Fact]
    public void LogBatchWorkflowProgress_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var completedCount = 5;
        var totalCount = 10;
        var correlationId = Guid.NewGuid();

        // Act & Assert
        WorkflowLogMessages.LogBatchWorkflowProgress(logger, completedCount, totalCount, correlationId);
    }

    [Fact]
    public void LogBatchWorkflowCompleted_ShouldNotThrow()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var successCount = 9;
        var totalCount = 10;
        var correlationId = Guid.NewGuid();
        var durationMs = 5000L;

        // Act & Assert
        WorkflowLogMessages.LogBatchWorkflowCompleted(logger, successCount, totalCount, correlationId, durationMs);
    }
}
