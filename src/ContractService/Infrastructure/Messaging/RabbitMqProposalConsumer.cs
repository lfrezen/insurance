using System.Text;
using System.Text.Json;

using ContractService.Application.DTOs;
using ContractService.Application.UseCases;
using ContractService.Domain.Ports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContractService.Infrastructure.Messaging;

public sealed class RabbitMqProposalConsumer : IMessageConsumer, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqProposalConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel? _channel;
    private bool _disposed;

    private const string ExchangeName = "insurance-events";
    private const string QueueName = "contract-service.proposal-approved";
    private const string RoutingKey = "proposal.approved";

    public RabbitMqProposalConsumer(
        IServiceProvider serviceProvider,
        ILogger<RabbitMqProposalConsumer> logger,
        IConnection connection)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _connection = connection;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting RabbitMQ ProposalApproved consumer...");

        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<ProposalApprovedEvent>(json);

                if (@event != null)
                {
                    _logger.LogInformation(
                        "Received ProposalApprovedEvent for Proposal {ProposalId}",
                        @event.ProposalId);

                    await ProcessEventAsync(@event, cancellationToken);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);

                    _logger.LogInformation(
                        "Successfully processed ProposalApprovedEvent for Proposal {ProposalId}",
                        @event.ProposalId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ProposalApprovedEvent");

                await _channel.BasicNackAsync(ea.DeliveryTag, false, false, cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ ProposalApproved consumer started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping RabbitMQ ProposalApproved consumer...");

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        _logger.LogInformation("RabbitMQ ProposalApproved consumer stopped");
    }

    private async Task ProcessEventAsync(ProposalApprovedEvent @event, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<CreateContractUseCase>();

        var request = new CreateContractRequest
        {
            ProposalId = @event.ProposalId
        };

        var result = await useCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning(
                "Failed to create contract for Proposal {ProposalId}: {Error}",
                @event.ProposalId,
                result.Error);
            throw new InvalidOperationException($"Failed to create contract: {result.Error}");
        }

        _logger.LogInformation(
            "Contract {ContractId} created successfully for Proposal {ProposalId}",
            result.Value?.Id,
            @event.ProposalId);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
        }

        _disposed = true;
    }
}
