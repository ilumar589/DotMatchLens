using DotMatchLens.Core.Contracts;
using MassTransit;

namespace DotMatchLens.Predictions.Sagas;

/// <summary>
/// Saga that orchestrates the match prediction workflow.
/// Coordinates between data gathering, prediction generation, and result persistence.
/// </summary>
public sealed class PredictionSaga : MassTransitStateMachine<PredictionSagaState>
{
    public State? Requested { get; private set; }
    public State? Processing { get; private set; }
    public State? Completed { get; private set; }
    public State? Failed { get; private set; }

    public Event<MatchPredictionRequested>? PredictionRequested { get; private set; }
    public Event<MatchPredictionCompleted>? PredictionCompleted { get; private set; }

    public PredictionSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => PredictionRequested!, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => PredictionCompleted!, x => x.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(PredictionRequested)
                .Then(context =>
                {
                    context.Saga.MatchId = context.Message.MatchId;
                    context.Saga.AdditionalContext = context.Message.AdditionalContext;
                    context.Saga.RequestedAt = context.Message.RequestedAt;
                    context.Saga.RetryCount = 0;
                })
                .TransitionTo(Requested)
                .ThenAsync(context => Console.Out.WriteLineAsync(
                    $"Prediction requested for match {context.Message.MatchId}"))
        );

        During(Requested,
            When(PredictionCompleted)
                .Then(context =>
                {
                    context.Saga.PredictionId = context.Message.PredictionId;
                    context.Saga.Confidence = context.Message.Confidence;
                    context.Saga.CompletedAt = context.Message.CompletedAt;
                    
                    if (!context.Message.Success)
                    {
                        context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    }
                })
                .If(context => context.Message.Success,
                    x => x.TransitionTo(Completed).Finalize())
                .If(context => !context.Message.Success,
                    x => x.TransitionTo(Failed))
        );

        SetCompletedWhenFinalized();
    }
}
