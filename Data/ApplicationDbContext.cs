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
            // Configure Appointment entity
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
        }
    }
}
