using Microsoft.EntityFrameworkCore;

using ProposalService.Infrastructure.Persistence;

namespace ProposalService.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
        await dbContext.Database.MigrateAsync();

        return app;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowAll");
        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
}
