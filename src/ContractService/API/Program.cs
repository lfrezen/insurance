using System.Text.Json.Serialization;

using ContractService.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();

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

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddRabbitMqConfiguration(builder.Configuration);
builder.Services.AddUseCases();

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.ConfigurePipeline();

await app.RunAsync();
