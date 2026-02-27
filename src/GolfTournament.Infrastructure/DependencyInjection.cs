using GolfTournament.Domain.Entities;
using GolfTournament.Domain.Interfaces;
using GolfTournament.Infrastructure.ExternalServices;
using GolfTournament.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GolfTournament.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Redis — AbortOnConnectFail=false so the API starts even when Redis isn't running locally
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.AbortOnConnectFail = false;
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisOptions));
        }

        // External service stubs (replace with real implementations when API credentials are available)
        services.AddScoped<IHandicapProvider, ManualHandicapProvider>();
        services.AddScoped<ICourseDataProvider, StubCourseDataProvider>();

        return services;
    }
}
