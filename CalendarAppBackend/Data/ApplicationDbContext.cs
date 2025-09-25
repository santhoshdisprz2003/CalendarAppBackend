using Microsoft.EntityFrameworkCore;
using CalendarAppBackend.Models;
using System.Diagnostics.CodeAnalysis;


namespace CalendarAppBackend.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>().ToTable("Appointments");

            // Relationship: one User → many Appointments
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Ensure UTC storage
            modelBuilder.Entity<Appointment>()
                .Property(a => a.StartTime)
                .HasConversion(
                    v => v.UtcDateTime,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );

            modelBuilder.Entity<Appointment>()
                .Property(a => a.EndTime)
                .HasConversion(
                    v => v.UtcDateTime,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );
        }

    }
}
