using Microsoft.EntityFrameworkCore;
using TransportApi.Models;

namespace TransportApi.Data
{
    public class FleetDbContext : DbContext
    {
        public FleetDbContext(DbContextOptions<FleetDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Driver> Drivers { get; set; } = null!;
        public DbSet<TransportOrder> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Usunięto modelBuilder.Entity<Vehicle>().HasKeyless(), które powodowało błąd kompilacji.

            // Wymuszamy, aby pojazdy i kierowcy mieli unikalne numery
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.LicenseNumber)
                .IsUnique();

            // Ustawienie relacji 1:N z TransportOrder (Vehicle)
            modelBuilder.Entity<TransportOrder>()
                .HasOne(o => o.Vehicle)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VehicleId);

            // Ustawienie relacji 1:N z TransportOrder (Driver)
            modelBuilder.Entity<TransportOrder>()
                .HasOne(o => o.Driver)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DriverId);

            // Konfiguracja dziedziczenia dla Vehicle (Table Per Hierarchy)
            modelBuilder.Entity<Vehicle>()
                .HasDiscriminator<string>("VehicleType")
                .HasValue<Truck>("Truck")
                .HasValue<Van>("Van");

            base.OnModelCreating(modelBuilder);
        }
    }
}