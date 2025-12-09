using MassTransit;

namespace DotMatchLens.Predictions.Sagas;

/// <summary>
/// State for the prediction saga.
/// Tracks the lifecycle of a match prediction request.
/// </summary>
public sealed class PredictionSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    
    // Match information
    public Guid MatchId { get; set; }
    public string? AdditionalContext { get; set; }
    
    // Prediction result
    public Guid? PredictionId { get; set; }
    public float? Confidence { get; set; }
    
    // Timing
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Error handling
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}
