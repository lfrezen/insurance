using ContractService.Domain.Ports;

namespace ContractService.API.Workers;

public class MessageConsumerWorker : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<MessageConsumerWorker> _logger;

    public MessageConsumerWorker(
        IMessageConsumer consumer,
        ILogger<MessageConsumerWorker> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageConsumerWorker starting...");

        try
        {
            await _consumer.StartAsync(stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "MessageConsumerWorker is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in MessageConsumerWorker. The service will terminate.");

            Environment.Exit(1);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MessageConsumerWorker stopping...");

        await _consumer.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);

        _logger.LogInformation("MessageConsumerWorker stopped");
    }
}
