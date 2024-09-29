using City_Taxi.Data;
using City_Taxi.Interfaces;
using City_Taxi.Models;
using City_Taxi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace City_Taxi.Repository
{
    public class PassengerCardDetailsRepository : IPassengerCardDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        public PassengerCardDetailsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool Add(PassengerCardDetails passengerCardDetails)
        {
            _context.Add(passengerCardDetails);
            return Save();
        }
        public bool Update(PassengerCardDetails passengerCardDetails)
        {
            _context.Update(passengerCardDetails);
            return Save();
        }
        public bool Delete(PassengerCardDetails passengerCardDetails)
        {
            _context.Remove(passengerCardDetails);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public async Task<IEnumerable<PassengerCardDetails>> GetAll()
        {
            return await _context.PassengerCardDetails.ToListAsync();
        }
        public async Task<PassengerCardDetails> GetByIdAsync(int id)
        {
            return await _context.PassengerCardDetails.FirstOrDefaultAsync(i => i.CardID == id);
        }
        public IEnumerable<CardViewModel> GetCardsByPassengerIdPMV(int passengerId)
        {
            var cards = _context.PassengerCardDetails
                .Where(c => c.PassengerID == passengerId)
                .Select(c => new CardViewModel
                {
                    CardId = c.CardID,
                    LastFourDigits = c.CardNumber.Substring(c.CardNumber.Length - 4)
                })
                .ToList();

            return cards;
        }
    }
}
