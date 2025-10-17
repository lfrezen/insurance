using ContractService.API.Workers;
using ContractService.Application.UseCases;
using ContractService.Domain.Ports;
using ContractService.Infrastructure.HttpClients;
using ContractService.Infrastructure.Messaging;
using ContractService.Infrastructure.Persistence;
using ContractService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using Polly;
using Polly.Extensions.Http;

using RabbitMQ.Client;

namespace ContractService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ContractDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IProposalClient, ProposalHttpClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ProposalService:BaseUrl"]
                ?? "http://localhost:5009");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"[Polly Retry] Attempt {retryCount} after {timespan.TotalSeconds}s delay. Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    Console.WriteLine($"[Polly Circuit Breaker] Circuit opened for {duration.TotalSeconds}s. Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("[Polly Circuit Breaker] Circuit closed. Requests will be allowed.");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[Polly Circuit Breaker] Circuit half-open. Testing with next request.");
                });
    }

    public static IServiceCollection AddRabbitMqConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqPort = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var rabbitMqUsername = configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = rabbitMqHost,
                Port = rabbitMqPort,
                UserName = rabbitMqUsername,
                Password = rabbitMqPassword
            };
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<IMessageConsumer, RabbitMqProposalConsumer>();
        services.AddHostedService<MessageConsumerWorker>();

        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateContractUseCase>();
        services.AddScoped<GetContractByIdUseCase>();
        services.AddScoped<GetAllContractsUseCase>();

        return services;
    }

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}
