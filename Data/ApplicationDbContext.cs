using City_Taxi.Models;
using City_Taxi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Login> Logins { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<VehicleTypes> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<DriverStatus> DriverStatuses { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Banks> Banks { get; set; }
        public DbSet<BankBranches> BankBranches { get; set; }
        public DbSet<PassengerCardDetails> PassengerCardDetails { get; set; }
        public DbSet<DriverRatings> DriverRatings { get; set; }
    }
}
