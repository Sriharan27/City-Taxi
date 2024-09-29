using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(Reservation reservation)
        {
            _context.Add(reservation);
            return Save();
        }
        public bool Update(Reservation reservation)
        {
            _context.Update(reservation);
            return Save();
        }
        public bool Delete(Reservation reservation)
        {
            _context.Remove(reservation);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public async Task<Reservation> GetWaitingReservationByDriverIDAsync(int driverID)
        {
            return await _context.Reservations.FirstOrDefaultAsync(r => r.DriverID == driverID && r.ReservationStatus == "Waiting for driver to accept");
        }
        public async Task<Reservation> GetReservationByReservationIDAsync(int reservationID)
        {
            return await _context.Reservations.Include(r => r.Passenger).Include(r => r.Driver).Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.ReservationID == reservationID);
        }
        public async Task<string> GetReservationStatusByReservationIDAsync(int reservationID)
        {
            return await _context.Reservations.Where(r => r.ReservationID == reservationID).Select(r => r.ReservationStatus).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Reservation>> GetReservationByDriverIDAsync(int driverID)
        {
            return await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .Where(r => r.ReservationStatus == "Trip Ended" && r.DriverID == driverID)
                .ToListAsync();
        }
        public async Task<IEnumerable<Reservation>> GetReservationByPassengerIDAsync(int passengerID)
        {
            return await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .Where(r => r.ReservationStatus == "Trip Ended" && r.PassengerID == passengerID)
                .ToListAsync();
        }
        public async Task<IEnumerable<Reservation>> GetAll()
        {
            return await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Driver)
                .Include(r => r.Vehicle)
                .Where(r => r.ReservationStatus == "Trip Ended")
                .ToListAsync();
        }
        public async Task<int> GetTripCountAsync()
        {
            return await _context.Reservations.Where(r => r.ReservationStatus == "Trip Ended").CountAsync();
        }
    }
}
