using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class DriverRatingsRepository : IDriverRatingsRepository
    {
        private readonly ApplicationDbContext _context;

        public DriverRatingsRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        public bool Add(DriverRatings driverRatings)
        {
            _context.Add(driverRatings);
            return Save();
        }
        public bool Update(DriverRatings driverRatings)
        {
            _context.Update(driverRatings);
            return Save();
        }
        public bool Delete(DriverRatings driverRatings)
        {
            _context.Remove(driverRatings);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public async Task<int> GetRatingsCountAsync()
        {
            return await _context.DriverRatings.CountAsync();
        }
        public async Task<int> GetTripStarsTotalAsync()
        {
            return await _context.DriverRatings.SumAsync(r => r.Stars);
        }
        public async Task<DriverRatings> GetRatingByReservationID(int reservationID)
        {
            return await _context.DriverRatings.FirstOrDefaultAsync(r => r.ReservationID == reservationID);
        }
    }
}
