using System;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public static class ApplicationBuilderExtensions
{
    public static void ApplyMigration(this IApplicationBuilder app)
    {

        using var scope = app.ApplicationServices.CreateScope();

        var service = scope.ServiceProvider;
        var loggerFactory = service.GetRequiredService<ILoggerFactory>();

        try
        {
            var context = service.GetRequiredService<ApplicationDbContext>();

            context.Database.Migrate();

        }
        catch (Exception ex)
        {

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "Error en migracion");
        }

    }
}

