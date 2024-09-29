using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

namespace City_Taxi.Repository
{
    public class PassengerRepository : IPassengerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;


        public PassengerRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public bool AddPassenger(Passenger passenger)
        {
            _context.Add(passenger);
            return Save();
        }
        public bool Update(Passenger passenger)
        {
            _context.Update(passenger);
            return Save();
        }
        public bool Delete(Passenger passenger)
        {
            _context.Remove(passenger);
            return Save();
        }

        public async Task<IEnumerable<Passenger>> GetAll()
        {
            return await _context.Passengers.ToListAsync();
        }

        public async Task<Passenger> GetByIdAsync(int id)
        {
            return await _context.Passengers.FirstOrDefaultAsync(i => i.PassengerID == id);
        }
        public async Task<int> GetPassengerCountAsync()
        {
            return await _context.Passengers.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        /*-------------------------------------------------------------*/
        public async Task UpdatePassengerLocationAsync(int passengerId, string latitude, string longitude)
        {
            var passenger = await GetByIdAsync(passengerId);
            if (passenger != null)
            {
                passenger.CurrentLatitude = latitude;
                passenger.CurrentLongitude = longitude;
                _context.Passengers.Update(passenger);
            }
        }
        // Calculate distance using the Haversine formula
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of Earth in kilometers
            var lat = (lat2 - lat1) * (Math.PI / 180);
            var lng = (lon2 - lon1) * (Math.PI / 180);
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2);
            var h2 = Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) * Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h = h1 + h2;
            var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            return R * c; // Distance in kilometers
        }
        public async Task<IEnumerable<DriverInfo>> GetNearbyDriversByTypeAsync(string latitude, string longitude, string vehicleType, double radiusKm)
        {
            // Define your connection string (this should come from your configuration)
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL query to fetch the necessary data
            string query = @"
                    SELECT DS.DriverStatusID, DS.DriverID, DS.Status, DS.Latitude, DS.Longitude, 
                            D.FirstName, D.LastName, VT.VehicleType, VT.PricePerKM, V.VehicleMake, 
                            V.VehicleModel, V.VehicleNumberPlate, V.VehicleID
                    FROM Vehicles V
                    INNER JOIN VehicleTypes VT ON VT.VehicleTypeID = V.VehicleTypeID
                    INNER JOIN DriverStatuses DS ON DS.DriverID = V.DriverID
                    INNER JOIN Drivers D ON D.DriverID = V.DriverID
                    WHERE VT.VehicleType = @VehicleType
                    AND DS.Status = 'AVAILABLE'";

            var drivers = new List<DriverInfo>();

            // Use SqlConnection and SqlCommand to interact directly with the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Open the connection asynchronously

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to prevent SQL injection
                    command.Parameters.Add(new SqlParameter("@VehicleType", vehicleType));

                    // Execute the query asynchronously
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync()) // Read data row by row
                        {
                            // Map each row to a DriverInfo object
                            var driverInfo = new DriverInfo
                            {
                                DriverStatusID = reader.GetInt32(0),
                                DriverID = reader.GetInt32(1),
                                Status = reader.GetString(2),
                                Latitude = reader.GetString(3),
                                Longitude = reader.GetString(4),
                                FirstName = reader.GetString(5),
                                LastName = reader.GetString(6),
                                VehicleType = reader.GetString(7),
                                PricePerKM = reader.GetDecimal(8),
                                VehicleMake = reader.GetString(9),
                                VehicleModel = reader.GetString(10),
                                VehicleNumberPlate = reader.GetString(11),
                                VehicleID = reader.GetInt32(12),
                            };

                            drivers.Add(driverInfo);
                        }
                    }
                }
            }

            // Filter drivers based on the radius from the passenger's current location
            var nearbyDrivers = drivers.Where(d => CalculateDistance(double.Parse(latitude), double.Parse(longitude),
                                                    double.Parse(d.Latitude),
                                                    double.Parse(d.Longitude)) <= radiusKm);

            return nearbyDrivers;
        }

    }
}
