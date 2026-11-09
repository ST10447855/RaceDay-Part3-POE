using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }

        public DbSet<Result> Results { get; set; }

        public DbSet<Organiser> Organisers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent multiple cascade paths in SQL Server for Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}