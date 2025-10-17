using ContractService.API.Workers;
using ContractService.Application.UseCases;
using ContractService.Domain.Ports;
using ContractService.Infrastructure.HttpClients;
using ContractService.Infrastructure.Messaging;
using ContractService.Infrastructure.Persistence;
using ContractService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

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
        });

        return services;
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
