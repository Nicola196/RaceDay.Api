using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

namespace RaceDay.Api.Tests;

public class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"RaceDayTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_TEST_ONLY_KEY_THAT_IS_LONG_ENOUGH_1234567890",
                ["Jwt:Issuer"] = "RaceDayApi",
                ["Jwt:Audience"] = "RaceDayClient",
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // ---- STEP 1: Remove EVERY trace of the SQL Server DbContext ----
            // EF Core 9/10 registers these; missing any one of them leaves
            // the SQL Server provider's internal service provider alive and
            // causes "Only a single database provider can be registered".

            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(RaceDayDbContext) ||
                d.ServiceType == typeof(IDbContextOptionsConfiguration<RaceDayDbContext>) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition().FullName != null &&
                 d.ServiceType.GetGenericTypeDefinition().FullName.Contains("IDbContextOptionsConfiguration"))
            ).ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // Belt-and-braces: RemoveAll for the common ones
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();

            // ---- STEP 2: Register the InMemory provider ----
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // ---- STEP 3: Swap in the fake weather service ----
            services.RemoveAll<IWeatherService>();
            services.AddSingleton<IWeatherService, FakeWeatherService>();

            // ---- STEP 4: Build the test database and seed it ----
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}