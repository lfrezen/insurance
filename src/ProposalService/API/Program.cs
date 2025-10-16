using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using ProposalService.Application.UseCases;
using ProposalService.Domain.Ports;
using ProposalService.Infrastructure.Persistence;
using ProposalService.Infrastructure.Repositories;

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

builder.Services.AddScoped<CreateProposalUseCase>();
builder.Services.AddScoped<GetProposalByIdUseCase>();
builder.Services.AddScoped<GetAllProposalsUseCase>();
builder.Services.AddScoped<ChangeProposalStatusUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
