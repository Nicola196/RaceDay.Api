using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleName)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventEnrolment>()
            .HasIndex(ee => new { ee.UserId, ee.EventId })
            .IsUnique();

        modelBuilder.Entity<EventEnrolment>()
            .HasOne(ee => ee.User)
            .WithMany(u => u.EventEnrolments)
            .HasForeignKey(ee => ee.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventEnrolment>()
            .HasOne(ee => ee.Event)
            .WithMany(e => e.EventEnrolments)
            .HasForeignKey(ee => ee.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.EventEnrolment)
            .WithOne(ee => ee.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Organiser" },
            new Role { RoleId = 2, RoleName = "Participant" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Under 20" },
            new Category { CategoryId = 2, CategoryName = "Senior" },
            new Category { CategoryId = 3, CategoryName = "10km" },
            new Category { CategoryId = 4, CategoryName = "21km" }
        );
    }
}