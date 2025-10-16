namespace ProposalService.Domain.Ports;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string routingKey, CancellationToken cancellationToken = default)
        where TEvent : class;
}
