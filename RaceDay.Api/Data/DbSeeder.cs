using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(RaceDayDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    FullName = "Olivia Organiser",
                    Email = "organiser@raceday.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    RoleId = 1
                },
                new User
                {
                    FullName = "Peter Participant",
                    Email = "participant@raceday.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    RoleId = 2
                }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Events.AnyAsync())
        {
            db.Events.AddRange(
                new Event
                {
                    EventName = "Rosebank City Run",
                    EventDescription = "A flat 10km through the city centre.",
                    EventDate = DateTime.UtcNow.AddDays(30),
                    Location = "Rosebank, Johannesburg",
                    Latitude = -26.1450,
                    Longitude = 28.0400,
                    Distance = "10km",
                    EventType = "Run",
                    CategoryId = 3
                },
                new Event
                {
                    EventName = "Rosebank Family Walk",
                    EventDescription = "A relaxed 5km walk for the whole family.",
                    EventDate = DateTime.UtcNow.AddDays(45),
                    Location = "Zoo Lake, Johannesburg",
                    Latitude = -26.1660,
                    Longitude = 28.0330,
                    Distance = "5km",
                    EventType = "Walk",
                    CategoryId = 1
                },
                new Event
                {
                    EventName = "Rosebank Cycle Challenge",
                    EventDescription = "A tough 21km cycle around the northern suburbs.",
                    EventDate = DateTime.UtcNow.AddDays(60),
                    Location = "Northcliff, Johannesburg",
                    Latitude = -26.1420,
                    Longitude = 27.9430,
                    Distance = "21km",
                    EventType = "Cycle",
                    CategoryId = 4
                }
            );
            await db.SaveChangesAsync();
        }
    }
}