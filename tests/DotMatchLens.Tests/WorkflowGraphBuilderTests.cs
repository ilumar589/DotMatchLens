using DotMatchLens.Predictions.UI;

namespace DotMatchLens.Tests;

/// <summary>
/// Tests for workflow graph builder.
/// </summary>
public sealed class WorkflowGraphBuilderTests
{
    [Fact]
    public void BuildMatchPredictionGraph_WithValidData_ShouldReturnGraph()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid();
        var status = "completed";
        var startedAt = DateTime.UtcNow.AddMinutes(-1);
        var completedAt = DateTime.UtcNow;
        var events = new List<WorkflowEventDto>
        {
            new(Guid.NewGuid().ToString(), workflowId, "started", "receive_request", startedAt, null),
            new(Guid.NewGuid().ToString(), workflowId, "completed", "receive_request", startedAt.AddSeconds(1), null)
        };

        // Act
        var graph = WorkflowGraphBuilder.BuildMatchPredictionGraph(
            workflowId, matchId, status, startedAt, completedAt, events);

        // Assert
        Assert.NotNull(graph);
        Assert.Equal(workflowId, graph.WorkflowId);
        Assert.Equal("match_prediction", graph.WorkflowType);
        Assert.Equal(status, graph.Status);
        Assert.Equal(startedAt, graph.StartedAt);
        Assert.Equal(completedAt, graph.CompletedAt);
        Assert.NotEmpty(graph.Nodes);
        Assert.NotEmpty(graph.Edges);
    }

    [Fact]
    public void BuildMatchPredictionGraph_ShouldHaveCorrectNumberOfNodes()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid();
        var events = new List<WorkflowEventDto>();

        // Act
        var graph = WorkflowGraphBuilder.BuildMatchPredictionGraph(
            workflowId, matchId, "pending", DateTime.UtcNow, null, events);

        // Assert
        Assert.Equal(7, graph.Nodes.Count); // start, receive_request, fetch_match, invoke_agent, save_prediction, publish_result, end
    }

    [Fact]
    public void BuildMatchPredictionGraph_ShouldHaveCorrectNumberOfEdges()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid();
        var events = new List<WorkflowEventDto>();

        // Act
        var graph = WorkflowGraphBuilder.BuildMatchPredictionGraph(
            workflowId, matchId, "pending", DateTime.UtcNow, null, events);

        // Assert
        Assert.Equal(6, graph.Edges.Count);
    }

    [Fact]
    public void BuildMatchPredictionGraph_WithNullWorkflowId_ShouldThrow()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var events = new List<WorkflowEventDto>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            WorkflowGraphBuilder.BuildMatchPredictionGraph(
                null!, matchId, "pending", DateTime.UtcNow, null, events));
    }

    [Fact]
    public void BuildMatchPredictionGraph_WithEmptyWorkflowId_ShouldThrow()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var events = new List<WorkflowEventDto>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            WorkflowGraphBuilder.BuildMatchPredictionGraph(
                "", matchId, "pending", DateTime.UtcNow, null, events));
    }

    [Fact]
    public void BuildMatchPredictionGraph_WithNullEvents_ShouldThrow()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            WorkflowGraphBuilder.BuildMatchPredictionGraph(
                workflowId, matchId, "pending", DateTime.UtcNow, null, null!));
    }

    [Fact]
    public void BuildBatchPredictionGraph_WithValidData_ShouldReturnGraph()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var batchSize = 10;
        var status = "running";
        var startedAt = DateTime.UtcNow.AddMinutes(-1);
        var completedCount = 5;

        // Act
        var graph = WorkflowGraphBuilder.BuildBatchPredictionGraph(
            workflowId, batchSize, status, startedAt, null, completedCount);

        // Assert
        Assert.NotNull(graph);
        Assert.Equal(workflowId, graph.WorkflowId);
        Assert.Equal("batch_prediction", graph.WorkflowType);
        Assert.Equal(status, graph.Status);
        Assert.Equal(startedAt, graph.StartedAt);
        Assert.Null(graph.CompletedAt);
        Assert.NotEmpty(graph.Nodes);
        Assert.NotEmpty(graph.Edges);
    }

    [Fact]
    public void BuildBatchPredictionGraph_ShouldHaveCorrectNumberOfNodes()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();

        // Act
        var graph = WorkflowGraphBuilder.BuildBatchPredictionGraph(
            workflowId, 10, "running", DateTime.UtcNow, null, 5);

        // Assert
        Assert.Equal(5, graph.Nodes.Count); // start, receive_batch, process_batch, aggregate_results, end
    }

    [Fact]
    public void BuildBatchPredictionGraph_ShouldHaveCorrectNumberOfEdges()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();

        // Act
        var graph = WorkflowGraphBuilder.BuildBatchPredictionGraph(
            workflowId, 10, "running", DateTime.UtcNow, null, 5);

        // Assert
        Assert.Equal(4, graph.Edges.Count);
    }

    [Fact]
    public void BuildBatchPredictionGraph_ShouldIncludeProgressMetadata()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();
        var batchSize = 10;
        var completedCount = 5;

        // Act
        var graph = WorkflowGraphBuilder.BuildBatchPredictionGraph(
            workflowId, batchSize, "running", DateTime.UtcNow, null, completedCount);

        // Assert
        Assert.NotNull(graph.Metadata);
        Assert.Contains("batchSize", graph.Metadata.Keys);
        Assert.Contains("completedCount", graph.Metadata.Keys);
        Assert.Contains("progress", graph.Metadata.Keys);
        Assert.Equal(batchSize, graph.Metadata["batchSize"]);
        Assert.Equal(completedCount, graph.Metadata["completedCount"]);
        Assert.Equal(0.5, graph.Metadata["progress"]); // 5/10 = 0.5
    }

    [Fact]
    public void BuildBatchPredictionGraph_WithZeroBatchSize_ShouldReturnZeroProgress()
    {
        // Arrange
        var workflowId = Guid.NewGuid().ToString();

        // Act
        var graph = WorkflowGraphBuilder.BuildBatchPredictionGraph(
            workflowId, 0, "running", DateTime.UtcNow, null, 0);

        // Assert
        Assert.NotNull(graph.Metadata);
        Assert.Equal(0.0, graph.Metadata["progress"]);
    }

    [Fact]
    public void BuildBatchPredictionGraph_WithNullWorkflowId_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            WorkflowGraphBuilder.BuildBatchPredictionGraph(
                null!, 10, "running", DateTime.UtcNow, null, 5));
    }

    [Fact]
    public void BuildBatchPredictionGraph_WithEmptyWorkflowId_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            WorkflowGraphBuilder.BuildBatchPredictionGraph(
                "", 10, "running", DateTime.UtcNow, null, 5));
    }
}
