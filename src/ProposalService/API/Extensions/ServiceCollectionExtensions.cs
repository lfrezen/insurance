using FluentValidation;

using Microsoft.EntityFrameworkCore;

using ProposalService.Application.UseCases;
using ProposalService.Application.Validators;
using ProposalService.Domain.Ports;
using ProposalService.Infrastructure.Messaging;
using ProposalService.Infrastructure.Persistence;
using ProposalService.Infrastructure.Repositories;

using RabbitMQ.Client;

namespace ProposalService.API.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDatabaseConfiguration(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<ProposalDbContext>(options =>
        options.UseNpgsql(connectionString));

    return services;
  }

  public static IServiceCollection AddRepositories(this IServiceCollection services)
  {
    services.AddScoped<IProposalRepository, ProposalRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
  }

  public static IServiceCollection AddValidators(this IServiceCollection services)
  {
    services.AddValidatorsFromAssemblyContaining<CreateProposalRequestValidator>();

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

    services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

    return services;
  }

  public static IServiceCollection AddUseCases(this IServiceCollection services)
  {
    services.AddScoped<CreateProposalUseCase>();
    services.AddScoped<GetProposalByIdUseCase>();
    services.AddScoped<GetAllProposalsUseCase>();
    services.AddScoped<ChangeProposalStatusUseCase>();

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
