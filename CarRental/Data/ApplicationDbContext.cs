using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;

namespace CarRental.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vehicle → Owner
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → Vehicle
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking → Renter
            modelBuilder.Entity<Booking>()
             .HasOne(b => b.Renter)
             .WithMany(u => u.Bookings)
             .HasForeignKey(b => b.RenterId)
             .OnDelete(DeleteBehavior.Restrict);
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
