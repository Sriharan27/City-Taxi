using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class DriverStatusRepository : IDriverStatusRepository
    {
        private readonly ApplicationDbContext _context;

        public DriverStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddStatus(DriverStatus driverStatus)
        {
            _context.Add(driverStatus);
            return Save();
        }
        public bool UpdateStatus(DriverStatus driverStatus)
        {
            _context.Update(driverStatus);
            return Save();
        }
        public bool DeleteStatus(DriverStatus driverStatus)
        {
            _context.Remove(driverStatus);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public async Task<DriverStatus> GetByDriverStatusIdAsync(int id)
        {
            return await _context.DriverStatuses.Include(d => d.Driver).FirstOrDefaultAsync(i => i.DriverStatusID == id);
        }

        // Retrieve a driver's current status
        public async Task<DriverStatus> GetDriverStatusAsync(int driverID)
        {
            return await _context.DriverStatuses.Include(d => d.Driver).FirstOrDefaultAsync(d => d.DriverID == driverID);
        }

        // Retrieve drivers near a specific location within a given radius
        public async Task<IEnumerable<DriverStatus>> GetNearbyDriversAsync(string latitude, string longitude, double radius)
        {
            // Calculate nearby drivers using Haversine formula to find distance between two GPS points
            return await _context.DriverStatuses
                .Where(d => GetDistance(double.Parse(d.Latitude), double.Parse(d.Longitude), double.Parse(latitude), double.Parse(longitude)) <= radius)
                .Include(d => d.Driver)
                .ToListAsync();
        }

        // Helper method to calculate distance between two GPS points
        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c; // Distance in kilometers
            return distance;
        }

        private double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
