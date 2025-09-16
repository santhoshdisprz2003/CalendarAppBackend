using Microsoft.EntityFrameworkCore;
using CalendarAppBackend.Models;

namespace CalendarAppBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>().ToTable("Appointments");

            // ✅ Ensure UTC storage
            modelBuilder.Entity<Appointment>()
                .Property(a => a.StartTime)
                .HasConversion(
                    v => v.UtcDateTime,   // store as UTC
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
