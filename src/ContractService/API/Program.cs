using ContractService.Application.UseCases;
using ContractService.Domain.Ports;
using ContractService.Infrastructure.HttpClients;
using ContractService.Infrastructure.Persistence;
using ContractService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<ContractDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient<IProposalClient, ProposalHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ProposalService:BaseUrl"]
        ?? "http://localhost:5009");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<CreateContractUseCase>();
builder.Services.AddScoped<GetContractByIdUseCase>();
builder.Services.AddScoped<GetAllContractsUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
