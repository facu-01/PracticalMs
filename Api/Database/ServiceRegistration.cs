using System;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public static class ServiceRegistration
{

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Db"));
        });

        return services;
    }

}
