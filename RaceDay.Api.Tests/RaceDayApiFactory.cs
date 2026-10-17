using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        builder.ConfigureServices(services =>
        {
            // 1. Remove the real SQL Server DbContext completely
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();

            // 2. Add the fast In-Memory DbContext
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // 3. Replace the Weather Service with our fake one
            services.RemoveAll<IWeatherService>();
            services.AddSingleton<IWeatherService, FakeWeatherService>();

            // 4. CRITICAL FIX: Ensure the schema and HasData (Roles/Categories) are created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            db.Database.EnsureCreated();
        });
    }
}