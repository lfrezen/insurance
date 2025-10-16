using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using ProposalService.Application.UseCases;
using ProposalService.Domain.Ports;
using ProposalService.Infrastructure.Messaging;
using ProposalService.Infrastructure.Persistence;
using ProposalService.Infrastructure.Repositories;

using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProposalDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqPort = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672");
var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddSingleton<IConnection>(sp =>
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

builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

builder.Services.AddScoped<CreateProposalUseCase>();
builder.Services.AddScoped<GetProposalByIdUseCase>();
builder.Services.AddScoped<GetAllProposalsUseCase>();
builder.Services.AddScoped<ChangeProposalStatusUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
