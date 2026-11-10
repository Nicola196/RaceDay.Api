using Microsoft.EntityFrameworkCore;
using RaceDayApi.Models;

namespace RaceDayApi.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Primary keys whose names don't follow the EF naming convention
        modelBuilder.Entity<EventEnrolment>().HasKey(e => e.EnrolmentId);
        modelBuilder.Entity<RaceResult>().HasKey(r => r.ResultId);

        // Seed the two roles so registration always has valid RoleIds
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Organiser" },
            new Role { RoleId = 2, RoleName = "Member" });

        // Users: unique email, one role per user
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Events: one category groups many events
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .Property(e => e.DistanceKm)
            .HasPrecision(8, 2);

        // Enrolments: a user can enrol in an event only once
        modelBuilder.Entity<EventEnrolment>()
            .HasIndex(e => new { e.UserId, e.EventId })
            .IsUnique();

        // Results: one result per enrolment (one-to-one, optional)
        modelBuilder.Entity<RaceResult>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<RaceResult>(r => r.EnrolmentId);
    }
}