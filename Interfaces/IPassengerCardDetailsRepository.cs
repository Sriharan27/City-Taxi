using City_Taxi.Models;
using City_Taxi.ViewModel;

namespace City_Taxi.Interfaces
{
    public interface IPassengerCardDetailsRepository
    {
        Task<IEnumerable<PassengerCardDetails>> GetAll();
        Task<PassengerCardDetails> GetByIdAsync(int id);
        IEnumerable<CardViewModel> GetCardsByPassengerIdPMV(int passengerId);
        bool Add(PassengerCardDetails passengerCardDetails);
        bool Update(PassengerCardDetails passengerCardDetails);
        bool Delete(PassengerCardDetails passengerCardDetails);
        bool Save();
    }
}
